using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public abstract class AnimatedClipAsset : PlayableAsset
		{
			[NonSerialized]
			public double _cachedDuration;

			public abstract void AddDefaultCurves(TimelineClip clip);
		}
	}
}