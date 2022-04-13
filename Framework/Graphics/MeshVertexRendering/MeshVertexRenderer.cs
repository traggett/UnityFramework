using UnityEngine;

namespace Framework
{
	namespace Graphics
	{
		public class MeshVertexRenderer : MonoBehaviour
		{
			#region Public Data
			public Renderer _sourceMesh;
			public Shader _vertexBakingShader;
			public Shader _vertexBakingReplacementShader;
			public LayerProperty _vertexBakingLayer;
			public MaterialRef[] _targetMaterials;
			#endregion

			#region Protected Data
			protected static int[] kAllowedTextureSizes = { 64, 128, 256, 512, 1024, 2048, 4098 };
			protected RenderTexture _vertexPositionBuffer;
			protected Camera _vertexBufferCamera;
			#endregion
			
			#region Unity Messages
			private void Start()
			{
				CreateVertexBuffer();
				CreateCamera();			
				SetupSkinnedMesh();
				SetupMaterials();
			}

			private void LateUpdate()
			{
#if UNITY_EDITOR
				SetupMaterials();
#endif
				RenderVertexBuffer();
			}

			void OnDestroy()
			{
				if (_vertexBufferCamera != null) Destroy(_vertexBufferCamera.gameObject);
				if (_vertexPositionBuffer != null) Destroy(_vertexPositionBuffer);
			}
			#endregion

			#region Puclic Interface
			public static int GetTextureSize(int numVerts)
			{
				int width = 0;

				for (int i = 0; i < kAllowedTextureSizes.Length; i++)
				{
					if (kAllowedTextureSizes[i] * kAllowedTextureSizes[i] > numVerts)
					{
						width = kAllowedTextureSizes[i];
						break;
					}
				}

				return width;
			}
			#endregion

			#region Protected Functions
			protected int GetVertexCount()
			{
				if (_sourceMesh is SkinnedMeshRenderer)
				{
					return ((SkinnedMeshRenderer)_sourceMesh).sharedMesh.vertexCount;
				}
				else if (_sourceMesh is MeshRenderer)
				{
					MeshFilter meshFilter = _sourceMesh.GetComponent<MeshFilter>();
					return meshFilter.sharedMesh.vertexCount;
				}

				return 0;
			}

			protected virtual void RenderVertexBuffer()
			{
				_vertexBufferCamera.Render();
			}

			protected virtual void CreateVertexBuffer()
			{
				int avatarVertCount = GetVertexCount();

				//Work out min square texture to contain the verts
				int textureSize = GetTextureSize(avatarVertCount);
				_vertexPositionBuffer = new RenderTexture(textureSize, textureSize, 0, RenderTextureFormat.ARGBFloat);
				_vertexPositionBuffer.Create();
			}

			protected virtual void SetupMaterials()
			{
				for (int i=0; i<_targetMaterials.Length; i++)
				{
					_targetMaterials[i].GetMaterial().SetTexture("_VertexPositions", _vertexPositionBuffer);
					_targetMaterials[i].GetMaterial().SetFloat("_VertexPositionsSize", _vertexPositionBuffer.width);
				}
			}

			protected virtual void CreateCamera()
			{
				GameObject cameraObj = new GameObject("Avatar Vertex Camera");
				cameraObj.transform.parent = this.transform;
				cameraObj.hideFlags = HideFlags.HideInHierarchy;

				cameraObj.transform.localPosition = Vector3.zero;
				cameraObj.transform.localRotation = Quaternion.identity;

				_vertexBufferCamera = cameraObj.AddComponent<Camera>();

				_vertexBufferCamera.enabled = false;

				_vertexBufferCamera.clearFlags = CameraClearFlags.SolidColor;
				_vertexBufferCamera.renderingPath = RenderingPath.Forward;
				_vertexBufferCamera.backgroundColor = Color.clear;
				_vertexBufferCamera.depth = -10000;
				_vertexBufferCamera.cullingMask = _vertexBakingLayer.GetLayerMask();

				_vertexBufferCamera.nearClipPlane = -100;
				_vertexBufferCamera.farClipPlane = 100;
				_vertexBufferCamera.orthographic = true;
				_vertexBufferCamera.orthographicSize = 100;
				_vertexBufferCamera.targetTexture = _vertexPositionBuffer;
				_vertexBufferCamera.allowMSAA = false;
				_vertexBufferCamera.allowHDR = false;
				_vertexBufferCamera.useOcclusionCulling = false;
				_vertexBufferCamera.stereoTargetEye = StereoTargetEyeMask.None;

				_vertexBufferCamera.SetReplacementShader(_vertexBakingShader, "VertexBaking");
			}

			protected virtual void SetupSkinnedMesh()
			{
				_sourceMesh.material = new Material(_vertexBakingReplacementShader);
				_sourceMesh.gameObject.layer = _vertexBakingLayer;	
			}
			#endregion
		}
	}
}
