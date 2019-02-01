using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[TrackClipType(typeof(ParentBindingMasterClipAsset))]
		public abstract class ParentBindingTrack : TrackAsset
		{
			protected List<IParentBindable> _boundTracks;

			public static void OnBindableCreated(PlayableGraph graph, PlayableBehaviour mixer, PlayableAsset parent)
			{
				ParentBindingTrack parentTrack = parent as ParentBindingTrack;
				IParentBindable child = mixer as IParentBindable;

				if (parentTrack != null && child != null)
				{
					parentTrack._boundTracks.Add(child);
				}
			}

			protected void OnCreateTrackMixer(PlayableGraph graph)
			{
				EnsureMasterClipExists();
				ClampMasterClipToChildClips();
				_boundTracks = new List<IParentBindable>();
			}
		
			public TimelineClip GetMasterClip()
			{
				TimelineClip masterClip = null;

				foreach (TimelineClip clip in GetClips())
				{
					masterClip = clip;
					break;
				}

				return masterClip;
			}

			public IParentBindable[] GetBoundTracks()
			{
				return _boundTracks.ToArray();
			}

			public void EnsureMasterClipExists()
			{
				TimelineClip masterClip = GetMasterClip();

				if (masterClip == null)
				{
					masterClip = CreateDefaultClip();
				}
			}

			public void ClampMasterClipToChildClips()
			{
				TimelineClip masterClip = GetMasterClip();

				if (masterClip != null)
				{
					double startTime = double.MaxValue;
					double endTime = 0d;

					foreach (TrackAsset child in GetChildTracks())
					{
						foreach (TimelineClip clip in child.GetClips())
						{
							double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
							double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

							startTime = Math.Min(startTime, clipStart);
							endTime = Math.Max(endTime, clipStart + clipDuration);
						}
					}

					masterClip.start = startTime;
					masterClip.duration = endTime - startTime;
				}			
			}
		}
	}
}