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
			public AnimatorPlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(AnimatorParamClipAsset), "_testa", new AnimationCurve(new Keyframe(0f, 0f, 0f, 0f, 0f, 0f), new Keyframe((float)clip.duration, 1f, 0f, 0f, 0f, 0f)));
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<AnimatorPlayableBehaviour> playable = ScriptPlayable<AnimatorPlayableBehaviour>.Create(graph, new AnimatorPlayableBehaviour());
				AnimatorPlayableBehaviour clone = playable.GetBehaviour();

				clone._clipAsset = this;
				
				return playable;
			}
		}
	}
}
