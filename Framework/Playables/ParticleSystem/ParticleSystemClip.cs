using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class ParticleSystemClip : BaseAnimatedClip, ITimelineClipAsset
		{
			public ParticleSystemPlayableBehaviour _data;
			
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation; }
			}

			public override void AddDefaultCurves(TimelineClip clip)
			{
				clip.curves.SetCurve("", typeof(ParticleSystemClip), "_emissionRate", new AnimationCurve());
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<ParticleSystemPlayableBehaviour> playable = ScriptPlayable<ParticleSystemPlayableBehaviour>.Create(graph, _data);
				ParticleSystemPlayableBehaviour clone = playable.GetBehaviour();

				clone._Clip = this;

				return playable;
			}
		}
	}
}
