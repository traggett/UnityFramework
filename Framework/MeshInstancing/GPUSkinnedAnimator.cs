using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
        [RequireComponent(typeof(Animator))]
        public class GPUSkinnedAnimator : MonoBehaviour
		{
            #region Public Data
            public AnimationTextureRef _animationTexture;
            #endregion

            #region Private Data
            private Animator _animator;

            private struct AnimationData
            {
                public AnimationTexture.Animation _animation;
                public float _frame;
                public float _speed;

				public void SetAnimation(AnimationTexture.Animation animation)
				{
					_animation = animation;
					_frame = 0;
					_speed = 1.0f;
				}

				public void Clear()
				{
					_frame = -1;
				}

				public float GetCurrentFrame()
				{
					return _animation._startFrameOffset + _frame;
				}
            }
            private AnimationData _currentAnimation;
            private AnimationData _previousAnimation;
			private float _blend;
            #endregion
			
            #region MonoBehaviour
            private void Awake()
            {
                _animator = GetComponent<Animator>();
                Prepare();
            }

            private void Start()
			{
				//For now start playing first animation
				_currentAnimation.SetAnimation(_animationTexture.GetAnimations()[0]);
			}

			private void Update()
			{
				UpdateAnimator();
				UpdateAnimation(ref _currentAnimation);
				//UpdateAnimation(ref _previousAnimation);
			}
			#endregion

			#region Public Functions
			public float GetCurrentAnimationFrame()
			{
				return _currentAnimation.GetCurrentFrame();
			}

			public float GetPreviousAnimationFrame()
			{
				return _previousAnimation.GetCurrentFrame();
			}

			public float GetCurrentPreviousBlend()
			{
				return _blend;
			}
			#endregion

			#region Private Functions
			private void Prepare()
            {
                AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);

                List<KeyValuePair<AnimationClip, AnimationClip>> anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                foreach (AnimationClip origClip in overrideController.animationClips)
                {
                    AnimationClip overrideClip = new AnimationClip();
                    overrideClip.name = origClip.name;
                    anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(origClip, overrideClip));
                }

                overrideController.ApplyOverrides(anims);
                _animator.runtimeAnimatorController = overrideController;
            }

			private void UpdateAnimator()
			{
				//Check what state the animator is in, whats the correspsonding clip and what speed its playing
				AnimatorClipInfo[] clipInfo = _animator.GetCurrentAnimatorClipInfo(0);
				
			}

			private void UpdateAnimation(ref AnimationData animation)
            {
				animation._frame += Time.deltaTime * animation._animation._fps;

				if (Mathf.FloorToInt(animation._frame) >= animation._animation._totalFrames - 1)
				{
					animation._frame = 0;
				}
			}
			#endregion
        }
    }
}