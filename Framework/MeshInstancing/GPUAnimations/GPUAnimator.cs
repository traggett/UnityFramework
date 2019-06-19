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
				[Range(0,8)]
				public int _numAdditionalLayers;
				#endregion

				#region Private Data
				private static Avatar _dummyAvatar;
				private Animator _animator;
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private GPUAnimatorLayer _baseLayer;
				private GPUAnimatorLayer[] _additionalLayers;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<Animator>();
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_baseLayer = new GPUAnimatorLayer(_animator, 0);

					_additionalLayers = new GPUAnimatorLayer[_numAdditionalLayers];
					for (int i=0; i<_numAdditionalLayers; i++)
						_additionalLayers[i] = new GPUAnimatorLayer(_animator, i + 1);

					_onInitialise += Initialise;

					CachedTransformData(this.transform);
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
				public override float GetMainAnimationFrame()
				{
					return _baseLayer.GetMainAnimationFrame();
				}

				public override float GetMainAnimationWeight()
				{
					return _baseLayer.GetMainAnimationWeight();
				}

				public override float GetBackgroundAnimationFrame()
				{
					return _baseLayer.GetBackgroundAnimationFrame();
				}

				public override Bounds GetBounds()
				{
					if (_skinnedMeshRenderer != null)
						return _skinnedMeshRenderer.bounds;

					return new Bounds();
				}
				#endregion

				#region Public Interface
				public float GetAnimationFrame(int layer)
				{
					if (layer == 0)
						return _baseLayer.GetMainAnimationFrame();

					return _additionalLayers[layer].GetMainAnimationFrame();
				}

				public float GetMainAnimationWeight(int layer)
				{
					if (layer == 0)
						return _baseLayer.GetMainAnimationWeight();

					return _additionalLayers[layer].GetMainAnimationWeight();
				}

				public float GetBackgroundAnimationFrame(int layer)
				{
					if (layer == 0)
						return _baseLayer.GetBackgroundAnimationFrame();

					return _additionalLayers[layer].GetBackgroundAnimationFrame();
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