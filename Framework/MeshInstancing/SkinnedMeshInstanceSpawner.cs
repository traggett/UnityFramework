using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Framework
{
	using Maths;	
	using Utils;
	
	namespace MeshInstancing
	{
		public class SkinnedMeshInstanceSpawner : MonoBehaviour
		{
			#region Public Data
			public AnimationTextureRef _animationTexture;
			public PrefabResourceRef _prefab;
			public bool _sortByDepth;

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
			 
			#region Private Data
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
			private float[] _currentFrame;
			private SkinnedMeshRenderer[] _skinnedMeshes;
			private MaterialPropertyBlock _propertyBlock;
			private class RenderData
			{
				public int _index;
				public Matrix4x4 _transform;
				public float _zDist;
			}
			private RenderData[] _renderData;
			private List<RenderData> _renderedObjects;
			private Matrix4x4[] _renderedObjectTransforms;
			private GameObject _referencePrefab;
			private static readonly int kMaxMeshes = 1023;
			#endregion

			#region Monobehaviour
			private void Awake()
			{
				//Find list of meshes and materials from prefab
				_referencePrefab = _prefab.LoadAndInstantiatePrefab(this.transform);
				_referencePrefab.SetActive(false);

				if (_referencePrefab != null)
				{
					_skinnedMeshes = _referencePrefab.GetComponentsInChildren<SkinnedMeshRenderer>();

					for (int i=0; i<_skinnedMeshes.Length; i++)
					{
						_skinnedMeshes[i].sharedMesh = AnimationTexture.AddExtraMeshData(_skinnedMeshes[i].sharedMesh);

						for (int j = 0; j < _skinnedMeshes[i].materials.Length; j++)
						{
							_animationTexture.SetMaterialProperties(_skinnedMeshes[i].materials[j]);
						}
					}
				}

				_renderedObjects = new List<RenderData>(kMaxMeshes);
				_instanceData = new InstanceData[kMaxMeshes];
				_renderData = new RenderData[kMaxMeshes];
				for (int i = 0; i < _renderData.Length; i++)
					_renderData[i] = new RenderData();
				_renderedObjectTransforms = new Matrix4x4[kMaxMeshes];
				_currentFrame = new float[kMaxMeshes];

				_propertyBlock = new MaterialPropertyBlock();
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

			public GameObject SpawnObject(Vector3 position)
			{
				for (int i = 0; i < _instanceData.Length; i++)
				{
					if (!_instanceData[i]._active)
					{
						if (_instanceData[i]._gameObject == null)
						{
							_instanceData[i]._gameObject = Instantiate(_referencePrefab, this.transform);
							_instanceData[i]._gameObject.transform.position = position;
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
						AnimationTexture.Animation textureAnim = GetAnimation(animation._animationIndex);
						_instanceData[i]._animationIndex = animation._animationIndex;
						_instanceData[i]._currentFrame = Random.Range(0, textureAnim._totalFrames - 2);
						_instanceData[i]._animationSpeed = animation._speedRange.GetRandomValue();

						OnSpawnObject(ref _instanceData[i]);
						
						return _instanceData[i]._gameObject;
					}
				}

				return null;
			}
			#endregion

			#region Protected Functions
			protected void Render(Camera camera)
			{
				if (_skinnedMeshes == null || _instanceData == null)
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
						for (int j = 0; j < _skinnedMeshes[i].materials.Length; j++)
						{
							int subMesh = Math.Min(j, _skinnedMeshes[i].sharedMesh.subMeshCount - 1);
							Graphics.DrawMeshInstanced(_skinnedMeshes[i].sharedMesh, subMesh, _skinnedMeshes[i].materials[j], _renderedObjectTransforms, _renderedObjects.Count, _propertyBlock);
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
					for (int j = 0; j < _skinnedMeshes[i].materials.Length; j++)
					{
						_animationTexture.SetMaterialProperties(_skinnedMeshes[i].materials[j]);
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
						AnimationTexture.Animation animation = GetAnimation(_instanceData[i]._animationIndex);

						//Update current frame
						_instanceData[i]._currentFrame += Time.deltaTime * animation._fps * _instanceData[i]._animationSpeed;

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

			private AnimationTexture.Animation GetAnimation(int index)
			{
				return _animationTexture.GetAnimations()[index];
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

			private static readonly int kSearchNodes = 24;

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
