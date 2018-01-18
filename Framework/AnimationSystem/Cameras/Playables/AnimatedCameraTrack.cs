using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		[TrackColor(0.99f, 0.4f, 0.71372549019f)]
		[TrackClipType(typeof(AnimatedCameraSnapshotClip)), TrackClipType(typeof(AnimatedCameraTweenSnapshotsClip))]
		[TrackBindingType(typeof(AnimatedCamera))]
		public class AnimatedCameraTrack : TrackAsset
		{
			public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
			{
				ScriptPlayable<AnimatedCameraTrackMixer> playable = ScriptPlayable<AnimatedCameraTrackMixer>.Create(graph, inputCount);
				PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();

				AnimatedCameraTrackMixer animatedCameraMixerBehaviour = playable.GetBehaviour();

				if (animatedCameraMixerBehaviour != null)
				{
					animatedCameraMixerBehaviour.SetClips(playableDirector, GetClips());
				}

				return playable;
			}

			public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
			{
				base.GatherProperties(director, driver);
			}
		}
	}
}