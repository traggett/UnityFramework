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

				public float GetWorldBoundingSphereRadius()
				{
					return _animator.GetWorldBoundingSphereRadius();
				}

				public Vector3 GetWorldBoundingSphereCentre()
				{
					return _animator.GetWorldBoundingSphereCentre();
				}
				
				public Bounds GetBounds()
				{
					return _animator.GetBounds();
				}
				#endregion
			}
		}
    }
}