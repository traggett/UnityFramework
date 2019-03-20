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
			private TimelineClip _masterClip;

			protected void OnCreateTrackMixer(PlayableGraph graph)
			{
				_boundTracks = new List<IParentBindableTrackMixer>();
				_masterClip = null;
			}
		
			public TimelineClip GetMasterClip()
			{
				if (_masterClip == null)
				{
					foreach (TimelineClip clip in GetClips())
					{
						_masterClip = clip;
						break;
					}
				}

				return _masterClip;
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