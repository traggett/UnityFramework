using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class SkinnedMeshBlendshapeClip : BaseAnimatedClip, ITimelineClipAsset
		{
			public SkinnedMeshBlendshapePlayableBehaviour _data;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(SkinnedMeshBlendshapeClip), "_weight", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<SkinnedMeshBlendshapePlayableBehaviour> playable = ScriptPlayable<SkinnedMeshBlendshapePlayableBehaviour>.Create(graph, _data);
				SkinnedMeshBlendshapePlayableBehaviour clone = playable.GetBehaviour();
				clone._clip = this;
				return playable;
			}
		}
	}
}
