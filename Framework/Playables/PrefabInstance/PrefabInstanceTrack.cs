using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Framework.Utils;

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
				OnCreateTrackMixer(graph);
				GetMasterClip().displayName = "Prefab Instance";

				return TimelineUtils.CreateTrackMixer<PrefabInstanceTrackMixer>(this, graph, go, inputCount);
			}
		}
	}
}