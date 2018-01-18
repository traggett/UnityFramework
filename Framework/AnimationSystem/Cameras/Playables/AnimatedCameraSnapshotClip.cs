using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		[Serializable]
		public class AnimatedCameraSnapshotClip : PlayableAsset, ITimelineClipAsset
		{
			public ExposedReference<MonoBehaviour> _snapshot;
			
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				var playable = ScriptPlayable<AnimatedCameraPlayableBehaviour>.Create(graph, new AnimatedCameraPlayableBehaviour());
				AnimatedCameraPlayableBehaviour clone = playable.GetBehaviour();
				clone._snapshot = _snapshot.Resolve(graph.GetResolver()) as IAnimatedCameraStateSource;
				return playable;
			}
		}
	}
}
