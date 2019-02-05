using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[TrackColor(248f / 255f, 64f / 255f, 100f / 255f)]
		[TrackBindingType(typeof(ParticleSystem))]
		[TrackClipType(typeof(ParticleSystemClip))]
		public class ParticleSystemTrack : BaseTrackAsset
		{
			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				return TimelineUtils.CreateTrackMixer<ParticleSystemTrackMixer>(this, graph, go, inputCount);
			}
		}
	}
}