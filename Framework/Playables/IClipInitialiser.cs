using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public interface IClipInitialiser
		{
			void OnClipCreated(TimelineClip clip);
		}
	}
}