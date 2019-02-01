using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Framework.Utils;
using System.Collections.Generic;

namespace Framework
{
	namespace Playables
	{
		[TrackColor(66f / 255f, 134f / 255f, 255f / 255f)]
		public class PrefabInstanceTrack : ParentBindingTrack
		{
			public PrefabResourceRef _prefab;

			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				EnsureMasterClipExists();
				GetMasterClip().displayName = "Prefab Instance";

				_boundTracks = new List<IParentBindable>();

				return TimelineUtils.CreateTrackMixer<PrefabInstanceTrackMixer>(this, graph, go, inputCount);
			}
		}
	}
}