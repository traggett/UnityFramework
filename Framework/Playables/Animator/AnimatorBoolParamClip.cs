using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorBoolParamClip : BaseAnimatedClip, ITimelineClipAsset
		{
			public AnimatorBoolParamPlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(AnimatorBoolParamClip), "_value", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<AnimatorBoolParamPlayableBehaviour> playable = ScriptPlayable<AnimatorBoolParamPlayableBehaviour>.Create(graph, _data);
				AnimatorBoolParamPlayableBehaviour clone = playable.GetBehaviour();
				clone._Clip = this;
				return playable;
			}
		}
	}
}
