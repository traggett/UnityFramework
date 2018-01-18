using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		[Serializable]
		public class AnimatedCameraTweenSnapshotsClip : PlayableAsset, ITimelineClipAsset
		{
			public AnimatedCameraPlayableBehaviour _template = new AnimatedCameraPlayableBehaviour();

			public ExposedReference<MonoBehaviour> _startSnapshot;
			public ExposedReference<MonoBehaviour> _endSnapshot;
			public eInterpolation _easeType;

			public ClipCaps clipCaps
			{
				get { return ClipCaps.Blending | ClipCaps.Extrapolation; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				var playable = ScriptPlayable<AnimatedCameraPlayableBehaviour>.Create(graph, _template);
				AnimatedCameraPlayableBehaviour clone = playable.GetBehaviour();
				clone._snapshot = _startSnapshot.Resolve(graph.GetResolver()) as IAnimatedCameraStateSource;
				clone._snapshotTo = _endSnapshot.Resolve(graph.GetResolver()) as IAnimatedCameraStateSource;
				clone._easeType = _easeType;
				return playable;
			}
		}
	}
}
