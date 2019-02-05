using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public abstract class BaseAnimatedClip : PlayableAsset, IClipInitialiser
		{
			public abstract void AddDefaultCurves(TimelineClip clip);

			public void OnClipCreated(TimelineClip clip)
			{
				clip.CreateCurves("Clip Parameters");
				clip.curves.ClearCurves();
				AddDefaultCurves(clip);
			}
		}
	}
}