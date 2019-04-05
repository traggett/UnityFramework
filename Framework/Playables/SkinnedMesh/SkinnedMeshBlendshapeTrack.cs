using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[TrackColor(214f / 255f, 51f / 255f, 138f / 255f)]
		[TrackBindingType(typeof(SkinnedMeshRenderer))]
		[TrackClipType(typeof(SkinnedMeshBlendshapeClip))]
		public class SkinnedMeshBlendshapeTrack : AnimatorParamTrack, IParentBindableTrack
		{
			public int _blendShapeIndex;

			public static SkinnedMeshRenderer GetSkinnedMeshFromGameObject(GameObject gameObject)
			{
				SkinnedMeshRenderer skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();

				if (skinnedMesh == null)
					skinnedMesh = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

				return skinnedMesh;
			}

			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				ScriptPlayable<SkinnedMeshBlendshapeTrackMixer> playable = TimelineUtils.CreateTrackMixer<SkinnedMeshBlendshapeTrackMixer>(this, graph, go, inputCount);

				ParentBindableTrack.OnCreateTrackMixer(this, playable.GetBehaviour(), graph);

				return playable;
			}
		}
	}
}