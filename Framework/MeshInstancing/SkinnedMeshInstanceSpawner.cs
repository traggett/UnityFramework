using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	using Maths;
	using Utils;
	
	namespace MeshInstancing
	{
		using GPUAnimations;

		public class SkinnedMeshInstanceSpawner : MonoBehaviour
		{
			#region Public Data
			public GPUAnimationsRef _animationTexture;
			public PrefabResourceRef _prefab;
			public bool _sortByDepth;
			public ShadowCastingMode _shadowCastingMode;
			public bool _recieveShadows;
			public bool _stripInstanceChildren;

			public enum eFrustrumCulling
			{
				Off,
				Sphere,
				Bounds
			}
			public eFrustrumCulling _frustrumCulling;
			public float _sphereCullingRadius;

			[Serializable]
			public struct AnimationData
			{
				public int _animationIndex;
				[Range(0, 1)]
				public float _probability;
				public FloatRange _speedRange;
			}
			
			[HideInInspector]
			public AnimationData[] _animations;
			#endregion

			#region Protected Data
			protected static readonly int kMaxMeshes = 1023;
			protected struct InstanceData
			{
				public bool _active;
				public GameObject _gameObject;
				public Transform _transform;
				public int _animationIndex;
				public float _currentFrame;
				public float _animationSpeed;
				public SkinnedMeshRenderer[] _skinnedMeshes;
				public object _extraData;
			}
			protected InstanceData[] _instanceData;
			protected MaterialPropertyBlock _propertyBlock;
			protected class RenderData
			{
				public int _index;
				public Matrix4x4 _transform;
				public float _zDist;
			}
			protected List<RenderData> _renderedObjects;
			#endregion

			#region Private Data
			private static readonly int kSearchNodes = 24;
			
			private float[] _currentFrame;
			private SkinnedMeshRenderer[] _skinnedMeshes;
			private Material[][] _materials;
			private RenderData[] _renderData;
			private Matrix4x4[] _renderedObjectTransforms;
			private GameObject _referencePrefab;
			private GameObject _clone;
			#endregion

			#region Monobehaviour
			private void Awake()
			{
				//Create a reference prefab that will be used to render them
				{
					_referencePrefab = _prefab.LoadAndInstantiatePrefab(this.transform);
					_referencePrefab.SetActive(false);

					_skinnedMeshes = _referencePrefab.GetComponentsInChildren<SkinnedMeshRenderer>();
					_materials = new Material[_skinnedMeshes.Length][];

					for (int i = 0; i < _skinnedMeshes.Length; i++)
					{
						_materials[i] = _skinnedMeshes[i].materials;

						for (int j = 0; j < _materials[i].Length; j++)
						{
							_animationTexture.SetMaterialProperties(_materials[i][j]);
						}
					}
				}

				//Create stripped down clone that will be instantiated
				{
					_clone = _prefab.LoadAndInstantiatePrefab(this.transform);
					_clone.SetActive(false);

					if (_stripInstanceChildren)
					{
						//Delete all skinned meshes
						SkinnedMeshRenderer[] skinnedMeshes = _clone.GetComponentsInChildren<SkinnedMeshRenderer>();

						for (int i = 0; i < skinnedMeshes.Length; i++)
						{
							skinnedMeshes[i].enabled = false;
							skinnedMeshes[i].sharedMesh = null;
							skinnedMeshes[i].sharedMaterials = new Material[0];

							//Delete bones too
							GameObjectUtils.DeleteChildren(skinnedMeshes[i].rootBone);
						}
					}
				}		

				//Init data
				_renderedObjects = new List<RenderData>(kMaxMeshes);
				_instanceData = new InstanceData[kMaxMeshes];
				_renderData = new RenderData[kMaxMeshes];
				for (int i = 0; i < _renderData.Length; i++)
					_renderData[i] = new RenderData();
				_renderedObjectTransforms = new Matrix4x4[kMaxMeshes];
				_currentFrame = new float[kMaxMeshes];

				_propertyBlock = new MaterialPropertyBlock();
			}

			private void OnDisable()
			{
				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (_instanceData[i]._gameObject != null)
					{
						Destroy(_instanceData[i]._gameObject);
						_instanceData[i]._gameObject = null;
						_instanceData[i]._active = false;
					}
				}
			}

			private void Update()
			{
				UpdateAnimations();
				Render(Camera.main);
			}
			#endregion

			#region Public Interface
			public GameObject GetReferenceModel()
			{
				return _referencePrefab;
			}

			public GameObject SpawnObject(Vector3 position, Quaternion rotation, Vector3 scale)
			{
				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (!_instanceData[i]._active)
					{
						if (_instanceData[i]._gameObject == null)
						{
							_instanceData[i]._gameObject = Instantiate(_clone, this.transform);
							_instanceData[i]._gameObject.transform.position = position;
							_instanceData[i]._gameObject.transform.rotation = rotation;
							_instanceData[i]._gameObject.transform.localScale = scale;

							_instanceData[i]._gameObject.SetActive(true);

							_instanceData[i]._skinnedMeshes = _instanceData[i]._gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
							for (int j = 0; j < _instanceData[i]._skinnedMeshes.Length; j++)
							{
								_instanceData[i]._skinnedMeshes[j].enabled = false;
							}
							
							_instanceData[i]._transform = _instanceData[i]._skinnedMeshes[0].transform;
						}

						_instanceData[i]._active = true;

						AnimationData animation = PickRandomAnimation();
						GPUAnimations.GPUAnimations.Animation textureAnim = GetAnimation(animation._animationIndex);
						_instanceData[i]._animationIndex = animation._animationIndex;
						_instanceData[i]._currentFrame = Random.Range(0, textureAnim._totalFrames - 2);
						_instanceData[i]._animationSpeed = animation._speedRange.GetRandomValue();

						OnSpawnObject(ref _instanceData[i]);
						
						return _instanceData[i]._gameObject;
					}
				}

				return null;
			}

			public void DestorySpawnedObject(GameObject gameObject)
			{
				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (_instanceData[i]._gameObject == gameObject)
					{
						DestorySpawnedObject(i);
						break;
					}
				}
			}
			#endregion

			#region Protected Functions
			protected void Render(Camera camera)
			{
				if (camera == null || _skinnedMeshes == null || _instanceData == null)
					return;

				_renderedObjects.Clear();

				Plane[] planes = null;
				Vector3[] planeNormals = null;
				float[] planeDistances = null;

				Vector3 cameraPos = camera.transform.position;

				if (_frustrumCulling != eFrustrumCulling.Off)
				{
					planes = GeometryUtility.CalculateFrustumPlanes(camera);

					if (_frustrumCulling == eFrustrumCulling.Sphere)
					{
						planeNormals = new Vector3[planes.Length];
						planeDistances = new float[planes.Length];

						for (int i = 0; i < planes.Length; i++)
						{
							planeNormals[i] = planes[i].normal;
							planeDistances[i] = planes[i].distance;
						}
					}
				}

				for (int i = 0; i < _instanceData.Length; i++)
				{
					bool rendered = _instanceData[i]._active;

					//If frustum culling is enabled, check should draw this game object
					if (rendered && _frustrumCulling == eFrustrumCulling.Bounds)
					{
						rendered = AreBoundsInFrustrum(planes, ref _instanceData[i]);
					}
					else if (rendered && _frustrumCulling == eFrustrumCulling.Sphere)
					{
						Vector3 position = _instanceData[i]._transform.position;
						Vector3 scale = _instanceData[i]._transform.lossyScale;
						rendered = IsSphereInFrustrum(ref planes, ref planeNormals, ref planeDistances, ref position, _sphereCullingRadius * Mathf.Max(scale.x, scale.y, scale.z));
					}

					if (rendered)
					{
						_renderData[i]._index = i;
						_renderData[i]._transform = GetTransform(ref _instanceData[i]);

						if (_sortByDepth)
						{
							Vector3 position = new Vector3(_renderData[i]._transform.m03, _renderData[i]._transform.m13, _renderData[i]._transform.m23);
							_renderData[i]._zDist = (cameraPos - position).sqrMagnitude;
						}

						AddToSortedList(ref _renderData[i]);
					}
				}

				if (_renderedObjects.Count > 0)
				{
					UpdateProperties();
					FillTransformMatricies();
					
					for (int i = 0; i < _skinnedMeshes.Length; i++)
					{
						for (int j = 0; j < _materials[i].Length; j++)
						{
							int subMesh = Math.Min(j, _skinnedMeshes[i].sharedMesh.subMeshCount - 1);
							Graphics.DrawMeshInstanced(_skinnedMeshes[i].sharedMesh, subMesh, _materials[i][j], _renderedObjectTransforms, _renderedObjects.Count, _propertyBlock, _shadowCastingMode, _recieveShadows, this.gameObject.layer);
						}
					}
				}
			}

			protected virtual Matrix4x4 GetTransform(ref InstanceData instanceData)
			{
				return instanceData._transform.localToWorldMatrix;
			}

			protected virtual void UpdateProperties()
			{
#if UNITY_EDITOR
				//In editor always set shared data
				for (int i = 0; i < _skinnedMeshes.Length; i++)
				{
					for (int j = 0; j < _materials[i].Length; j++)
					{
						_animationTexture.SetMaterialProperties(_materials[i][j]);
					}
				}
#endif

				//Update property block
				int index = 0;
				foreach (RenderData renderData in _renderedObjects)
				{
					_currentFrame[index++] = _instanceData[renderData._index]._currentFrame;
				}

				//Update property block
				_propertyBlock.SetFloatArray("frameIndex", _currentFrame);
			}

			protected void UpdateAnimations()
			{
				//Update particle frame progress
				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (_instanceData[i]._active)
					{
						//Progress current animation
						GPUAnimations.GPUAnimations.Animation animation = GetAnimation(_instanceData[i]._animationIndex);

						//Update current frame
						float prevFrame = _instanceData[i]._currentFrame;
						_instanceData[i]._currentFrame += Time.deltaTime * animation._fps * _instanceData[i]._animationSpeed;

						//Check for events
						GPUAnimations.GPUAnimations.CheckForEvents(_instanceData[i]._gameObject, animation, prevFrame, _instanceData[i]._currentFrame);

						//Is animation finished?
						if (Mathf.FloorToInt(_instanceData[i]._currentFrame - animation._startFrameOffset) >= animation._totalFrames - 1)
						{
							AnimationData newAnimationData = PickRandomAnimation();
							animation = GetAnimation(newAnimationData._animationIndex);

							_instanceData[i]._animationIndex = newAnimationData._animationIndex;
							_instanceData[i]._currentFrame = animation._startFrameOffset;
							_instanceData[i]._animationSpeed = newAnimationData._speedRange.GetRandomValue();;
						}
					}
				}
			}

			protected virtual void OnSpawnObject(ref InstanceData instanceData)
			{

			}

			protected int GetSpawnedObjectCount()
			{
				int count = 0;

				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (_instanceData[i]._active)
					{
						count++;
					}
				}

				return count;
			}

			protected void DestorySpawnedObject(int index)
			{
				if (_instanceData[index]._gameObject != null)
				{
					Destroy(_instanceData[index]._gameObject);
					_instanceData[index]._gameObject = null;
				}
				_instanceData[index]._active = false;
			}
			#endregion

			#region Private Functions
			private bool AreBoundsInFrustrum(Plane[] cameraFrustrumPlanes, ref InstanceData instanceData)
			{
				//Only test first skinned mesh?
				return GeometryUtility.TestPlanesAABB(cameraFrustrumPlanes, instanceData._skinnedMeshes[0].bounds);
			}

			private AnimationData PickRandomAnimation()
			{
				float totalWeights = 0.0f;

				for (int i = 0; i < _animations.Length; i++)
				{
					totalWeights += _animations[i]._probability;
				}

				float chosen = Random.Range(0.0f, totalWeights);
				totalWeights = 0.0f;

				for (int i = 0; i < _animations.Length; i++)
				{
					totalWeights += _animations[i]._probability;

					if (chosen <= totalWeights)
						return _animations[i];
				}

				return _animations[0];
			}

			private GPUAnimations.GPUAnimations.Animation GetAnimation(int index)
			{
				return _animationTexture.GetAnimations()._animations[index];
			}

			private void FillTransformMatricies()
			{
				for (int i = 0; i < _renderedObjects.Count; i++)
				{
					_renderedObjectTransforms[i] = _renderedObjects[i]._transform;
				}
			}

			private void AddToSortedList(ref RenderData renderData)
			{
				int index = 0;

				if (_sortByDepth)
				{
					index = FindInsertIndex(renderData._zDist, 0, _renderedObjects.Count);
				}

				_renderedObjects.Insert(index, renderData);
			}

			private int FindInsertIndex(float zDist, int startIndex, int endIndex)
			{
				int searchWidth = endIndex - startIndex;
				int numSearches = Mathf.Min(kSearchNodes, searchWidth);
				int nodesPerSearch = Mathf.FloorToInt(searchWidth / (float)numSearches);

				int currIndex = startIndex;
				int prevIndex = currIndex;

				for (int i = 0; i < numSearches; i++)
				{
					//If this distance is greater than current node its between this and prev node
					if (zDist > _renderedObjects[currIndex]._zDist)
					{
						//If first node or search one node at a time then found our index
						if (i == 0 || nodesPerSearch == 1)
						{
							return currIndex;
						}
						//Otherwise its between this and the previous index
						else
						{
							return FindInsertIndex(zDist, prevIndex, currIndex);
						}
					}

					prevIndex = currIndex;
					currIndex = (i == numSearches - 1) ? endIndex : startIndex + ((i + 1) * nodesPerSearch);
				}

				return endIndex;
			}

			private static bool IsSphereInFrustrum(ref Plane[] frustrumPlanes, ref Vector3[] planeNormals, ref float[] planeDistances, ref Vector3 center, float radius, float frustumPadding = 0f)
			{
				for (int i = 0; i < frustrumPlanes.Length; i++)
				{
					float dist = planeNormals[i].x * center.x + planeNormals[i].y * center.y + planeNormals[i].z * center.z + planeDistances[i];

					if (dist < -radius - frustumPadding)
					{
						return false;
					}
				}

				return true;
			}
			#endregion
		}
	}
}
