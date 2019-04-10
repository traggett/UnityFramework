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
			private object _currentBinding;

			#region ITrackMixer
			public virtual void SetTrackAsset(TrackAsset trackAsset, PlayableDirector playableDirector)
			{
				_trackAsset = trackAsset;
				_director = playableDirector;
				_currentBinding = null;
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

			public override void OnPlayableDestroy(Playable playable)
			{
				ClearChildTrackBindings();
			}

			protected void SetChildTrackBindings(object playerData)
			{
				if (_currentBinding != playerData)
				{
					_currentBinding = playerData;
					ParentBindingTrack track = (ParentBindingTrack)_trackAsset;

					foreach (IParentBindableTrackMixer parentBindable in track.GetBoundTracks())
					{
						parentBindable.SetParentBinding(playerData);
					}
				}
			}

			protected void ClearChildTrackBindings()
			{
				ParentBindingTrack track = (ParentBindingTrack)_trackAsset;

				foreach (IParentBindableTrackMixer parentBindable in track.GetBoundTracks())
				{
					parentBindable.ClearParentBinding();
				}

				_currentBinding = null;
			}
		}
	}
}