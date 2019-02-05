using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[TrackClipType(typeof(ParentBindingMasterClip))]
		public abstract class ParentBindingTrack : BaseTrackAsset
		{
			private List<IParentBindableTrackMixer> _boundTracks;

			protected void OnCreateTrackMixer(PlayableGraph graph)
			{
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
				if (_boundTracks != null)
					_boundTracks.Add(boundTrack);
			}

#if UNITY_EDITOR
			public virtual UnityEngine.Object GetEditorBinding(PlayableDirector director)
			{
				return director.GetGenericBinding(this);
			}
#endif
		}
	}
}