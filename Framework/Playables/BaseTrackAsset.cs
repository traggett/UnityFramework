using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public abstract class BaseTrackAsset : TrackAsset
		{
			protected override void OnCreateClip(TimelineClip clip)
			{
				IClipInitialiser clipInitialiser = clip.asset as IClipInitialiser;

				if (clipInitialiser != null)
				{
					clipInitialiser.OnClipCreated(clip);
				}
			}
		}
	}
}