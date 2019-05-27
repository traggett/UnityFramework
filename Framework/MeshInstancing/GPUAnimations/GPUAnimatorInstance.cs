using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public interface IGPUAnimatorInstance
			{
				float GetCurrentAnimationFrame();
				float GetBlendedAnimationFrame();
				float GetAnimationBlend();
				float GetSphericalBoundsRadius();
				Matrix4x4 GetWorldMatrix();
				Vector3 GetWorldPos();
				Vector3 GetWorldScale();
				SkinnedMeshRenderer GetSkinnedMeshRenderer();
			}

			public struct GPUAnimatorInstance : IMeshInstance
			{
				public readonly GameObject _gameObject;
				public readonly IGPUAnimatorInstance _animator;

				public GPUAnimatorInstance(GameObject instance)
				{
					_gameObject = instance;
					_animator = _gameObject.GetComponent<IGPUAnimatorInstance>();
				}

				#region IMeshInstance
				public bool IsActive()
				{
					return _gameObject.activeSelf;
				}

				public bool IsValid()
				{
					return _gameObject != null;
				}

				public bool AreBoundsInFrustrum(Plane[] cameraFrustrumPlanes)
				{
					return GeometryUtility.TestPlanesAABB(cameraFrustrumPlanes, _animator.GetSkinnedMeshRenderer().bounds);
				}

				public float GetSphericalBoundsRadius()
				{
					return _animator.GetSphericalBoundsRadius();
				}

				public Matrix4x4 GetWorldMatrix()
				{
					return _animator.GetWorldMatrix();
				}

				public Vector3 GetWorldPos()
				{
					return _animator.GetWorldPos();
				}

				public Vector3 GetWorldScale()
				{
					return _animator.GetWorldScale();
				}
				#endregion
			}
		}
    }
}