using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[TrackClipType(typeof(ParentBindingMasterClipAsset))]
		public abstract class ParentBindingTrack : BaseTrackAsset
		{
			private List<IParentBindableTrackMixer> _boundTracks;

			protected void OnCreateTrackMixer(PlayableGraph graph)
			{
				EnsureMasterClipExists();
				_boundTracks = new List<IParentBindableTrackMixer>();
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

			public IParentBindableTrackMixer[] GetBoundTracks()
			{
				return _boundTracks.ToArray();
			}

			public void AddChildTrack(IParentBindableTrackMixer boundTrack)
			{
				_boundTracks.Add(boundTrack);
			}

#if UNITY_EDITOR
			public virtual UnityEngine.Object GetEditorBinding(PlayableDirector director)
			{
				return director.GetGenericBinding(this);
			}
#endif

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