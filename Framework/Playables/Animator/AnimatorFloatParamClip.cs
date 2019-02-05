using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorFloatParamClip : BaseAnimatedClip, ITimelineClipAsset
		{
			public AnimatorFloatParamPlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(AnimatorFloatParamClip), "_value", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<AnimatorFloatParamPlayableBehaviour> playable = ScriptPlayable<AnimatorFloatParamPlayableBehaviour>.Create(graph, _data);
				AnimatorFloatParamPlayableBehaviour clone = playable.GetBehaviour();
				clone._Clip = this;
				return playable;
			}
		}
	}
}
