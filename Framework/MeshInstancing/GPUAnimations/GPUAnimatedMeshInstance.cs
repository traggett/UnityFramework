using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public struct GPUAnimatedMeshInstance : IMeshInstance
			{
				public readonly GameObject _gameObject;
				public readonly GPUAnimatorBase _animator;

				public GPUAnimatedMeshInstance(GameObject instance)
				{
					_gameObject = instance;
					_animator = GameObjectUtils.GetComponent<GPUAnimatorBase>(instance, true);
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