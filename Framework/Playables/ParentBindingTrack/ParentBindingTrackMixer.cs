using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public abstract class ParentBindingTrackMixer : PlayableBehaviour, ITrackMixer
		{
			protected TrackAsset _trackAsset;
			protected PlayableDirector _director;

			#region ITrackMixer
			public virtual void SetTrackAsset(TrackAsset trackAsset, PlayableDirector playableDirector)
			{
				_trackAsset = trackAsset;
				_director = playableDirector;
			}

			public TrackAsset GetTrackAsset()
			{
				return _trackAsset;
			}
			#endregion

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				SetChildTrackBindings(playerData);
			}

			protected void SetChildTrackBindings(object playerData)
			{
				ParentBindingTrack track = (ParentBindingTrack)_trackAsset;

				foreach (IParentBindable parentBindable in track.GetBoundTracks())
				{
					parentBindable.SetBinding(playerData);
				}
			}
		}
	}
}