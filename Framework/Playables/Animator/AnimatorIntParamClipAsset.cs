using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorIntParamClipAsset : BaseAnimatedClipAsset, ITimelineClipAsset
		{
			public AnimatorIntParamPlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(AnimatorIntParamClipAsset), "_value", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<AnimatorIntParamPlayableBehaviour> playable = ScriptPlayable<AnimatorIntParamPlayableBehaviour>.Create(graph, _data);
				AnimatorIntParamPlayableBehaviour clone = playable.GetBehaviour();
				clone._clipAsset = this;
				return playable;
			}
		}
	}
}
