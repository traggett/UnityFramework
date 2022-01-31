using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Framework
{
	namespace AnimationSystem
	{
		[RequireComponent(typeof(Animator))]
		public class RemoteAnimator : MonoBehaviour
		{
			#region Public Data
			public Animator Animator
			{
				get
				{
					if (_animator == null)
					{
						_animator = GetComponent<Animator>();
					}

					return _animator;
				}
			}

			public AnimatorControllerPlayable AnimatorController
			{
				get
				{
					return _animatorControllerPlayable;
				}
			}

			public float Weight
			{
				get
				{
					return _weight;
				}
				set
				{
					_weight = value;
					_fadeSpeed = 0f;
				}
			}
			#endregion

			#region Private Data
			private Animator _animator;
			private PlayableGraph _playableGraph;
			private AnimationPlayableOutput _animationPlayableOutput;
			private AnimatorControllerPlayable _animatorControllerPlayable;
			private float _weight;
			private float _fadeSpeed;
			#endregion

			#region Public Inteface
			public void Play(Animator target, float fadeTime = -1f)
			{
				Stop();

				_playableGraph = PlayableGraph.Create(this.name);

				_animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "RemoteAnimator", target);

				_animatorControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, Animator.runtimeAnimatorController);
				_animationPlayableOutput.SetSourcePlayable(_animatorControllerPlayable);

				_weight = fadeTime > 0f ? 0f : 1f;
				_fadeSpeed = fadeTime > 0f ? 1f / fadeTime : 0f;
				_animationPlayableOutput.SetWeight(_weight);

				_playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
				_playableGraph.Play();
			}

			public void Stop(float fadeTime = -1f)
			{
				if (fadeTime > 0f)
				{
					_fadeSpeed = -1f / fadeTime;
				}
				else if (_playableGraph.IsValid())
				{
					_playableGraph.Stop();

					_animationPlayableOutput.SetTarget(null);
					_animationPlayableOutput = AnimationPlayableOutput.Null;

					_animatorControllerPlayable = AnimatorControllerPlayable.Null;

					_playableGraph.Destroy();
				}			
			}
			#endregion

			#region Unity Messages
			private void OnDisable()
			{
				Stop();
			}

			private void Update()
			{
				if (_playableGraph.IsValid())
				{
					if (_fadeSpeed > 0f && _weight < 1f)
					{
						_weight += _fadeSpeed * Time.deltaTime;
						_weight = Mathf.Clamp01(_weight);

						_animationPlayableOutput.SetWeight(_weight);
					}
					else if (_fadeSpeed < 0f && _weight > 0f)
					{
						_weight += _fadeSpeed * Time.deltaTime;
						
						if (_weight <= 0f)
						{
							Stop();
						}
						else
						{
							_animationPlayableOutput.SetWeight(_weight);
						}
					}
				}
			}
			#endregion
		}
	}
}