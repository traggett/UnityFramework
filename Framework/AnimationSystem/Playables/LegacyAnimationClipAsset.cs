using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		[Serializable]
		[NotKeyable]
		public class LegacyAnimationClipAsset : PlayableAsset, ITimelineClipAsset
		{
			public AnimationClip _animationClip;
			public double _animationDuration = PlayableBinding.DefaultDuration;
			public float _animationSpeed = 1.0f;

			private TimelineClip _clip;

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

			public void SetClip(TimelineClip clip)
			{
				_clip = clip;
			}

			public TimelineClip GetTimelineClip()
			{
				return _clip;
			}		

			public int GetChannel()
			{
				return ((LegacyAnimatorTrack)_clip.GetParentTrack())._animationChannel;
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

		}
	}
}
