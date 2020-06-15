using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


namespace Framework
{
	using Playables;

	namespace Paths
	{
		[TrackColor(255f / 255f, 64f / 255f, 0f / 255f)]
		[TrackClipType(typeof(PathMoveClipAsset))]
		[TrackBindingType(typeof(Transform))]
		public class PathTrack : TrackAsset
		{
			public int _animationChannel;

			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				return TimelineUtils.CreateTrackMixer<PathTrackMixer>(this, graph, go, inputCount);
			}
		}
	}
}