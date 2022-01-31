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
			[Range(0f, 1f)]
			public float _weight;
			#endregion

			#region Private Data
			private Animator _animator;
			private PlayableGraph _playableGraph;
			private AnimationPlayableOutput _animationPlayableOutput;
			#endregion

			#region Public Inteface
			public void Play(Animator target, float weight = 1f)
			{
				Stop();

				if (_animator == null)
				{
					_animator = GetComponent<Animator>();
				}

				_playableGraph = PlayableGraph.Create(this.name);

				_animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "RemoteAnimator", target);

				AnimatorControllerPlayable animatorControllerPlayable = AnimatorControllerPlayable.Create(_playableGraph, _animator.runtimeAnimatorController);
				_animationPlayableOutput.SetSourcePlayable(animatorControllerPlayable);

				_weight = weight;
				_animationPlayableOutput.SetWeight(_weight);

				_playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
				_playableGraph.Play();
			}

			public void Stop()
			{
				_playableGraph.Stop();

				_animationPlayableOutput.SetTarget(null);

				_playableGraph.Destroy();
			}
			#endregion

			#region Unity Messages
			private void Update()
			{
				if (_playableGraph.IsValid())
				{
					_animationPlayableOutput.SetWeight(_weight);
				}
			}
			#endregion
		}
	}
}