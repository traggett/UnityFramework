using Framework.Maths;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework
{
	namespace MeshInstancing
	{
		public class MeshInstancer : MonoBehaviour
		{
			#region Public Data
			public static readonly int kMaxInstances = 1023;

			public Mesh _mesh;
			public Material[] _materials;
			public ShadowCastingMode _shadowCastingMode;
			public bool _billboard;
			public float _boundRadius;
			public float _frustrumPadding;

			public delegate void OnMeshInstanceActivated(int index);
			public OnMeshInstanceActivated _onMeshInstanceActivated;
			public delegate void OnMeshInstanceWillBeRendered(int renderIndex, int instanceIndex);
			public OnMeshInstanceWillBeRendered _onMeshInstanceWillBeRendered;
			public delegate void OnUpdateMaterialPropertyBlock(MaterialPropertyBlock propertyBlock);
			public OnUpdateMaterialPropertyBlock _onUpdateMaterialPropertyBlock;
			public delegate void UpdateInstanceTransform(ref Matrix4x4 matrix);
			public UpdateInstanceTransform _updateInstanceTransform;
			#endregion

			#region Private Data
			protected MaterialPropertyBlock _propertyBlock;

			protected Matrix4x4[] _instanceTransforms;
			protected Vector3[] _instanceCachedScales;
			protected bool[] _instanceActive;

			protected int _numRenderedInstances;
			protected Matrix4x4[] _renderedInstanceTransforms;

			protected Plane[] _frustrumPlanes;
			protected Vector3[] _frustrumPlaneNormals;
			protected float[] _frustrumPlaneDistances;
			#endregion

			#region Monobehaviour
			private void Start()
			{
				InitialiseIfNeeded();
			}

			private void Update()
			{
				InitialiseIfNeeded();
				Render(Camera.main);
			}

			private void OnDisable()
			{
				if (_instanceActive != null)
				{
					for (int i = 0; i < kMaxInstances; i++)
					{
						_instanceActive[i] = false;
					}
				}
			}
			#endregion
		
			#region Public Interface
			public int GetNumInstances()
			{
				int numActiveInstances = 0;

				for (int i = 0; i < kMaxInstances; i++)
				{
					if (_instanceActive[i])
						numActiveInstances++;
				}

				return numActiveInstances;
			}

			public int AddInstance(Vector3 position, Quaternion rotation, Vector3 scale)
			{
				for (int i=0; i<kMaxInstances; i++)
				{
					if (!_instanceActive[i])
					{
						_instanceActive[i] = true;
						SetInstanceTransform(i, Matrix4x4.TRS(position, rotation, scale));
						_onMeshInstanceActivated?.Invoke(i);

						return i;
					}
				}
				
				return -1;
			}

			public void DeactiveInstance(int index)
			{
				_instanceActive[index] = false;
			}

			public bool IsInstanceActive(int index)
			{
				return _instanceActive[index];
			}

			public Matrix4x4 GetInstanceTransform(int index)
			{
				return _instanceTransforms[index];
			}

			public void SetInstanceTransform(int index, Matrix4x4 matrix)
			{
				_instanceTransforms[index] = matrix;
				_instanceCachedScales[index] = matrix.lossyScale;
			}

			public Vector3 GetInstancePosition(int index)
			{
				return new Vector3(_instanceTransforms[index].m03, _instanceTransforms[index].m13, _instanceTransforms[index].m23);
			}
			#endregion

			#region Protected Functions
			protected virtual void InitialiseIfNeeded()
			{
				if (_propertyBlock == null)
				{
					_propertyBlock = new MaterialPropertyBlock();
				}

				if (_instanceTransforms == null || _renderedInstanceTransforms == null)
				{ 
					_instanceTransforms = new Matrix4x4[kMaxInstances];
					_instanceCachedScales = new Vector3[kMaxInstances];
					_instanceActive = new bool[kMaxInstances];
					_renderedInstanceTransforms = new Matrix4x4[kMaxInstances];

					_frustrumPlanes = new Plane[6];
					_frustrumPlaneNormals = new Vector3[6];
					_frustrumPlaneDistances = new float[6];
				}
			}
			
			protected virtual void Render(Camera camera)
			{
				if (_mesh == null || _materials.Length < _mesh.subMeshCount)
					return;

				_numRenderedInstances = 0;

				_frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);

				for (int i = 0; i < _frustrumPlanes.Length; i++)
				{
					_frustrumPlaneNormals[i] = _frustrumPlanes[i].normal;
					_frustrumPlaneDistances[i] = _frustrumPlanes[i].distance;
				}

				Vector3 cameraPos = camera.transform.position;

				for (int i = 0; i < kMaxInstances; i++)
				{
					if (_instanceActive[i])
					{
						if (IsMeshInFrustrum(i))
						{
							_renderedInstanceTransforms[_numRenderedInstances] = GetModifiedTransform(i, cameraPos);
							_onMeshInstanceWillBeRendered?.Invoke(_numRenderedInstances, i);
							_numRenderedInstances++;
						}
					}
				}

				if (_numRenderedInstances > 0)
				{
					_onUpdateMaterialPropertyBlock?.Invoke(_propertyBlock);

					for (int i = 0; i < _mesh.subMeshCount; i++)
					{
						Graphics.DrawMeshInstanced(_mesh, i, _materials[i], _renderedInstanceTransforms, _numRenderedInstances, _propertyBlock, _shadowCastingMode);
					}
				}
			}

			protected Matrix4x4 GetModifiedTransform(int index, Vector3 cameraPos)
			{
				Matrix4x4 matrix = _instanceTransforms[index];

				_updateInstanceTransform?.Invoke(ref matrix);
				
				if (_billboard)
				{
					float rotationAngle = Vector3.Angle(Vector3.up, new Vector3(matrix.m01, matrix.m11, matrix.m21));

					Vector3 forward = (cameraPos - MathUtils.GetPosition(ref matrix)).normalized;
					Quaternion rotation = Quaternion.AngleAxis(rotationAngle, forward);

					Vector3 left = Vector3.Cross(forward, Vector3.up);
					Vector3 up = Vector3.Cross(left, forward);

					Vector3 scale = matrix.lossyScale;

					left = rotation * left * scale.x;
					matrix.m00 = left.x;
					matrix.m10 = left.y;
					matrix.m20 = left.z;

					up = rotation * up * scale.y;
					matrix.m01 = up.x;
					matrix.m11 = up.y;
					matrix.m21 = up.z;
					
					forward = rotation * forward * scale.z;
					matrix.m02 = forward.x;
					matrix.m12 = forward.y;
					matrix.m22 = forward.z;
				}

				return matrix;
			}

			protected bool IsMeshInFrustrum(int index)
			{
				float radius = _boundRadius * Mathf.Max(_instanceCachedScales[index].x, _instanceCachedScales[index].y, _instanceCachedScales[index].z);

				for (int i = 0; i < 6; i++)
				{
					float dist = _frustrumPlaneNormals[i].x * _instanceTransforms[index].m03 + _frustrumPlaneNormals[i].y * _instanceTransforms[index].m13 + _frustrumPlaneNormals[i].z * _instanceTransforms[index].m23 + _frustrumPlaneDistances[i];

					if (dist < -radius - _frustrumPadding)
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
