using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		[Serializable]
		[NotKeyable]
		public class LegacyAnimatorMasterClipAsset : PlayableAsset, ITimelineClipAsset
		{
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				//Never actually does anything - just created to ensure tracks mixer is created
				return new Playable();
			}
		}
	}
}
