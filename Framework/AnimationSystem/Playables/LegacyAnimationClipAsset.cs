using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		[Serializable]
		[NotKeyable]
		public class LegacyAnimationClipAsset : PlayableAsset, ITimelineClipAsset
		{
			public AnimationClip _animationClip;
			public double _animationDuration = PlayableBinding.DefaultDuration;
			public float _animationSpeed = 1.0f;

			protected LegacyAnimatorTrack _parentAnimatorTrack;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override double duration
			{
				get
				{
					if (_animationDuration <= 0.0f)
						return PlayableBinding.DefaultDuration;

					return _animationDuration;
				}
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<LegacyAnimatorPlayableBehaviour> playable = ScriptPlayable<LegacyAnimatorPlayableBehaviour>.Create(graph, new LegacyAnimatorPlayableBehaviour());
				LegacyAnimatorPlayableBehaviour clone = playable.GetBehaviour();
				
				clone._clipAsset = this;
				clone._animation = _animationClip;
				clone._animationSpeed = _animationSpeed;

				return playable;
			}

			public void SetParentTrack(LegacyAnimatorTrack track)
			{
				_parentAnimatorTrack = track;
			}

			public LegacyAnimatorTrack GetParentTrack()
			{
				return _parentAnimatorTrack;
			}
		}
	}
}
