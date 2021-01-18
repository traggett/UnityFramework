using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		[TrackColor(204f / 255f, 48f / 255f, 74f / 255f)]
		[TrackClipType(typeof(LegacyAnimationClipAsset))]
		public class LegacyAnimatorChannelTrack : TrackAsset
		{
			public int _animationChannel;

			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				ScriptPlayable<LegacyAnimatorChannelTrackMixer> playable = TimelineUtils.CreateTrackMixer<LegacyAnimatorChannelTrackMixer>(this, graph, go, inputCount);
				LegacyAnimatorTrack parentTrack = this.parent as LegacyAnimatorTrack;

				if (parentTrack != null)
				{
					IEnumerable<TimelineClip> clips = GetClips();

					foreach (TimelineClip clip in clips)
					{
						LegacyAnimationClipAsset animationClip = clip.asset as LegacyAnimationClipAsset;

						if (animationClip != null)
						{
							clip.displayName = animationClip._animationClip != null ? animationClip._animationClip.name : "Animation";
							animationClip.SetParentTrack(parentTrack);
						}
					}
				}

				return playable;
			}

			protected override void OnCreateClip(TimelineClip clip)
			{
				LegacyAnimatorTrack parentTrack = this.parent as LegacyAnimatorTrack;

				if (parentTrack != null)
				{
					LegacyAnimationClipAsset animationClip = clip.asset as LegacyAnimationClipAsset;

					if (animationClip != null)
					{
						animationClip.SetParentTrack(parentTrack);
					}
				}
			}
		}
	}
}