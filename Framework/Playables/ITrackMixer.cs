using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public interface ITrackMixer
		{
			TrackAsset GetTrackAsset();
			void SetTrackAsset(TrackAsset trackAsset, PlayableDirector playableDirector);
		}
	}
}