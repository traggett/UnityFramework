using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[TrackColor(255f / 255f, 0f / 255f, 255f / 255f)]
		[TrackBindingType(typeof(Material))]
		[TrackClipType(typeof(MaterialColorParamClip))]
		public class MaterialColorParamTrack : MaterialParamTrack, IParentBindableTrack
		{	
			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				ScriptPlayable<MaterialColorParamTrackMixer> playable = TimelineUtils.CreateTrackMixer<MaterialColorParamTrackMixer>(this, graph, go, inputCount);

				ParentBindableTrack.OnCreateTrackMixer(this, playable.GetBehaviour(), graph);

				return playable;
			}
		}
	}
}