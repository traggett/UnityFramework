using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		[TrackColor(204f / 255f, 48f / 255f, 74f / 255f)]
		[TrackBindingType(typeof(LegacyAnimator))]
		[TrackClipType(typeof(LegacyAnimationClipAsset), false)]
		public class LegacyAnimatorTrack : TrackAsset
		{
			public int _animationChannel;
			
			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				ScriptPlayable<LegacyAnimatorTrackMixer> playable = TimelineUtils.CreateTrackMixer<LegacyAnimatorTrackMixer>(this, graph, go, inputCount);
				LegacyAnimatorTrackMixer mixer = playable.GetBehaviour();

				SetClipReferences(this);

				List<int> channels = new List<int>();
				channels.Add(_animationChannel);

				foreach (TrackAsset child in GetChildTracks())
				{
					if (child is LegacyAnimatorTrack legacyAnimatorTrack)
					{
						channels.Add(legacyAnimatorTrack._animationChannel);
						SetClipReferences(legacyAnimatorTrack);
					}
				}

				mixer.SetChannels(channels.ToArray());
				

				return playable;
			}

			private static void SetClipReferences(LegacyAnimatorTrack asset)
			{
				foreach (TimelineClip clip in asset.GetClips())
				{
					LegacyAnimationClipAsset animationClipAsset = clip.asset as LegacyAnimationClipAsset;
					
					if (animationClipAsset != null)
					{
						animationClipAsset.SetClip(clip);
					}
				}
			}
		}
	}
}