using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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
			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				EnsureMasterClipExists();
				return TimelineUtils.CreateTrackMixer<LegacyAnimatorTrackMixer>(this, graph, go, inputCount);
			}

			public void EnsureMasterClipExists()
			{
				TimelineClip masterClip = null;

				foreach (TimelineClip clip in GetClips())
				{
					masterClip = clip;
					break;
				}

				if (masterClip == null)
				{
					masterClip = CreateDefaultClip();
				}

				//Set clips duration to match max duration of timeline
				masterClip.start = 0;
				masterClip.duration = 0;
				masterClip.duration = this.timelineAsset.duration;
				masterClip.displayName = GetMasterClipName();
			}

			protected virtual string GetMasterClipName()
			{
				return "(Legacy Animation)";
			}
		}
	}
}