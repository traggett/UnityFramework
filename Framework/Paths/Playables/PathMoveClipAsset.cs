using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Paths
	{
		[Serializable]
		[NotKeyable]
		public class PathMoveClipAsset : PlayableAsset, ITimelineClipAsset
		{
			public ExposedReference<Path> _path;
			
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<PathPlayableBehaviour> playable = ScriptPlayable<PathPlayableBehaviour>.Create(graph, new PathPlayableBehaviour());
				PathPlayableBehaviour clone = playable.GetBehaviour();

				clone._clipAsset = this;
				clone._path = _path.Resolve(graph.GetResolver());

				return playable;
			}
		}
	}
}
