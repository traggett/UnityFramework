using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorParamClipAsset : AnimatedClipAsset, ITimelineClipAsset
		{
			public AnimatorPlayableBehaviour _template;
			
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(AnimatorParamClipAsset), "_test", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<AnimatorPlayableBehaviour> playable = ScriptPlayable<AnimatorPlayableBehaviour>.Create(graph, _template);
				AnimatorPlayableBehaviour clone = playable.GetBehaviour();

				clone._clipAsset = this;
				
				return playable;
			}
		}
	}
}
