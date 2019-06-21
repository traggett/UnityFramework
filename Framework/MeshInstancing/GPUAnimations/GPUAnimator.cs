using UnityEngine;

namespace Framework
{
	using System;
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
				private GPUAnimatorLayer[] _layers;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_animator = GetComponent<Animator>();
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);

					int numLayers = Math.Min(1 + _numAdditionalLayers, _animator.layerCount);

					_layers = new GPUAnimatorLayer[numLayers];
					for (int i=0; i< _layers.Length; i++)
						_layers[i] = new GPUAnimatorLayer(_animator, i);

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
					return _layers[0].GetMainAnimationFrame();
				}

				public override float GetMainAnimationWeight()
				{
					return _layers[0].GetMainAnimationWeight();
				}

				public override float GetBackgroundAnimationFrame()
				{
					return _layers[0].GetBackgroundAnimationFrame();
				}

				public override Bounds GetBounds()
				{
					if (_skinnedMeshRenderer != null)
						return _skinnedMeshRenderer.bounds;

					return new Bounds();
				}
				#endregion

				#region Public Interface
				public float GetLayerWeight(int layer)
				{
					return _layers[layer].GetWeight();
				}

				public float GetMainAnimationFrame(int layer)
				{
					return _layers[layer].GetMainAnimationFrame();
				}

				public float GetMainAnimationWeight(int layer)
				{
					return _layers[layer].GetMainAnimationWeight();
				}

				public float GetBackgroundAnimationFrame(int layer)
				{
					return _layers[layer].GetBackgroundAnimationFrame();
				}

				public int GetNumLayers()
				{
					return _layers.Length;
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
					for (int i=0; i<_layers.Length; i++)
						_layers[i].Update();
				}

				private void UpdateRootMotion()
				{
					if (_animator.applyRootMotion)
					{
						_layers[0].GetRootMotionVelocities(out Vector3 velocity, out Vector3 angularVelocity);

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