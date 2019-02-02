using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public interface IParentBindableTrack
		{
			
		}

		public static class ParentBindableTrack
		{
			public static void OnCreateTrackMixer<TTrack, TMixer>(TTrack track, TMixer mixer, PlayableGraph graph) where TTrack : TrackAsset, IParentBindableTrack where TMixer : PlayableBehaviour, IParentBindableTrackMixer
			{
				if (track != null && mixer != null)
				{
					ParentBindingTrack parentTrack = track.parent as ParentBindingTrack;

					if (parentTrack != null)
					{
						parentTrack.AddChildTrack(mixer);
					}
				}
			}
		}
	}
}