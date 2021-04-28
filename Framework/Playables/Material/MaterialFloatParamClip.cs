using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class MaterialFloatParamClip : BaseAnimatedClip, ITimelineClipAsset
		{
			public MaterialFloatParamPlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(MaterialFloatParamClip), "_value", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<MaterialFloatParamPlayableBehaviour> playable = ScriptPlayable<MaterialFloatParamPlayableBehaviour>.Create(graph, _data);
				MaterialFloatParamPlayableBehaviour clone = playable.GetBehaviour();
				clone._clip = this;
				return playable;
			}
		}
	}
}
