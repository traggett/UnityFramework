using System;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			public abstract class GPUAnimatorBase : MonoBehaviour
			{
				#region Public Data
				public float _sphericalBoundsRadius;
				public Vector3 _sphericalBoundsCentre;
				public Action _onInitialise;
				#endregion

				#region Protected Data
				protected GPUAnimatorRenderer _renderer;
				protected bool _initialised;
				#endregion

				#region Private Data
				private Matrix4x4 _worldMatrix;
				private Vector3 _worldPos;
				private Vector3 _worldScale;
				private float _worldBoundingSphereRadius;
				private Vector3 _worldBoundingSphereCentre;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					UpdateCachedTransform();
					_initialised = false;
				}

				private void LateUpdate()
				{
					if (this.transform.hasChanged)
						UpdateCachedTransform();
				}

#if UNITY_EDITOR
				void OnDrawGizmosSelected()
				{
					UpdateCachedTransform();
					Gizmos.color = Color.yellow;
					Gizmos.DrawWireSphere(_worldBoundingSphereCentre, _worldBoundingSphereRadius);
				}
#endif
				#endregion

				#region Public Interface
				public void Initialise(GPUAnimatorRenderer renderer)
				{
					_initialised = true;
					_renderer = renderer;
					_onInitialise?.Invoke();
				}

				public GPUAnimatorRenderer GetRenderer()
				{
					return _renderer;
				}

				public Matrix4x4 GetWorldMatrix()
				{
					return _worldMatrix;
				}

				public Vector3 GetWorldPos()
				{
					return _worldPos;
				}

				public Vector3 GetWorldScale()
				{
					return _worldScale;
				}
				
				public float GetWorldBoundingSphereRadius()
				{
					return _worldBoundingSphereRadius;
				}

				public Vector3 GetWorldBoundingSphereCentre()
				{
					return _worldBoundingSphereCentre;
				}
				#endregion

				#region Abstract Interface
				public abstract float GetCurrentAnimationFrame();
				public abstract float GetCurrentAnimationWeight();
				public abstract float GetPreviousAnimationFrame();
				public abstract Bounds GetBounds();
				#endregion

				#region Protected Functions
				protected void UpdateCachedTransform()
				{
					_worldMatrix = this.transform.localToWorldMatrix;
					_worldPos = MathUtils.GetPosition(ref _worldMatrix);
					_worldScale = this.transform.lossyScale;
					_worldBoundingSphereRadius = Mathf.Max(Mathf.Max(_worldScale.x, _worldScale.y), _worldScale.z) * _sphericalBoundsRadius;
					_worldBoundingSphereCentre = _worldMatrix.MultiplyPoint3x4(_sphericalBoundsCentre);
				}
				#endregion
			}
		}
    }
}