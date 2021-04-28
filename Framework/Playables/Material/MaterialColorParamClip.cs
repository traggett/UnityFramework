using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class MaterialColorParamClip : BaseAnimatedClip, ITimelineClipAsset
		{
			public MaterialColorParamPlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(MaterialColorParamClip), "_value", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<MaterialColorParamPlayableBehaviour> playable = ScriptPlayable<MaterialColorParamPlayableBehaviour>.Create(graph, _data);
				MaterialColorParamPlayableBehaviour clone = playable.GetBehaviour();
				clone._clip = this;
				return playable;
			}
		}
	}
}
