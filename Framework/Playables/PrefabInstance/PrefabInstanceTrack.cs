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
			public ExposedReference<Transform> _prefabSpawnPoint;
			public bool _makePrefabSpawnPointParent;

			protected override void OnCreateClip(TimelineClip clip)
			{
				base.OnCreateClip(clip);
				clip.displayName = "Prefab Instance";
			}

			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				Transform spawnPoint = _prefabSpawnPoint.Resolve(graph.GetResolver());
				OnCreateTrackMixer(graph);

				ScriptPlayable<PrefabInstanceTrackMixer> mixer = TimelineUtils.CreateTrackMixer<PrefabInstanceTrackMixer>(this, graph, go, inputCount);
				mixer.GetBehaviour().SetSpawnPoint(spawnPoint, _makePrefabSpawnPointParent);

				return mixer;
			}

#if UNITY_EDITOR
			public override Object GetEditorBinding(PlayableDirector director)
			{
				return _prefab.LoadPrefab();
			}
#endif
		}
	}
}