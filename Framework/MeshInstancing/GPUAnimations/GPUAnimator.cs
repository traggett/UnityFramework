using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(Animator))]
			public class GPUAnimator : GPUAnimatorBase
			{
				#region Public Data
				[HideInInspector]
				public float _animatedValue;
				#endregion

				#region Private Data
				private static Avatar _dummyAvatar;
				private Animator _animator;
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private GPUAnimatorLayer _baseLayer;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<Animator>();
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_baseLayer = new GPUAnimatorLayer(_animator, 0);
					_onInitialise += Initialise;

					UpdateCachedTransform();
				}

				private void Update()
				{
					if (_initialised)
					{
						UpdateAnimator();
						UpdateRootMotion();
					}
				}
				#endregion

				#region GPUAnimatorBase
				public override float GetCurrentAnimationFrame()
				{
					return _baseLayer.GetCurrentAnimationFrame();
				}

				public override float GetCurrentAnimationWeight()
				{
					return _baseLayer.GetCurrentAnimationWeight();
				}

				public override float GetPreviousAnimationFrame()
				{
					return _baseLayer.GetPreviousAnimationFrame();
				}

				public override Bounds GetBounds()
				{
					if (_skinnedMeshRenderer != null)
						return _skinnedMeshRenderer.bounds;

					return new Bounds();
				}
				#endregion

				#region Private Functions
				private void Initialise()
				{
					_animator.runtimeAnimatorController = _renderer.GetOverrideControllerForAnimator(_animator);
					_animator.avatar = GetDummyAvatar();
				}
				
				private void UpdateAnimator()
				{
					_baseLayer.Update();
				}

				private void UpdateRootMotion()
				{
					if (_animator.applyRootMotion)
					{
						_baseLayer.GetRootMotionVelocities(out Vector3 velocity, out Vector3 angularVelocity);

						Quaternion rotation = this.transform.localRotation;
						Quaternion delta = Quaternion.Euler(angularVelocity * Time.deltaTime);
						rotation *= delta;

						Vector3 offset = velocity * Time.deltaTime;
						offset = rotation * offset;

						Vector3 position = this.transform.localPosition;
						position += offset;

						this.transform.localPosition = position;
						this.transform.localRotation = rotation;
					}
				}

				private static Avatar GetDummyAvatar()
				{
					if (_dummyAvatar == null)
					{
						GameObject temp = new GameObject();
						_dummyAvatar = AvatarBuilder.BuildGenericAvatar(temp, "");
						_dummyAvatar.name = "GPU Animated Avatar";
						Destroy(temp);
					}
				
					return _dummyAvatar;
				}
				#endregion
			}
		}
    }
}