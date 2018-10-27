using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public static class TimelineUtils
		{
			//Finds the TimelineClip corresponding to a playable asset in a parent track
			public static TimelineClip GetClip(TrackAsset track, IPlayableAsset clipAsset)
			{
				IEnumerable<TimelineClip> clips = track.GetClips();

				foreach (TimelineClip clip in clips)
				{
					if (clip.asset == (Object)clipAsset)
						return clip;
				}

				return null;
			}

			//Creates a track mixer for an ITrackMixer PlayableBehaviour
			public static ScriptPlayable<T> CreateTrackMixer<T>(TrackAsset track, PlayableGraph graph, GameObject go, int inputCount) where T : class, IPlayableBehaviour, ITrackMixer, new()
			{
				ScriptPlayable<T> playable = ScriptPlayable<T>.Create(graph, inputCount);
				ITrackMixer mixer = playable.GetBehaviour();

				if (mixer != null)
				{
					PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();
					mixer.SetTrackAsset(track, playableDirector);
				}

				return playable;
			}

			//Finds the track mixer for a track given a playable graph instance
			public static T GetTrackMixer<T>(PlayableGraph graph, TrackAsset track) where T : class, IPlayableBehaviour, ITrackMixer, new()
			{
				int rootCount = graph.GetRootPlayableCount();

				for (int i = 0; i < rootCount; i++)
				{
					Playable root = graph.GetRootPlayable(i);

					T trackMixer = GetTrackMixer<T>(root, track);

					if (trackMixer != null)
					{
						return trackMixer;
					}
				}

				return null;
			}

			private static T GetTrackMixer<T>(Playable root, TrackAsset track) where T : class, IPlayableBehaviour, ITrackMixer, new()
			{
				int inputCount = root.GetInputCount(); ;

				for (int i = 0; i < inputCount; i++)
				{
					Playable rootInput = root.GetInput(i);

					if (rootInput.IsValid())
					{
						//If this input is a SpineAnimatorTrackMixer, check it matches our track
						if (rootInput.GetPlayableType() == typeof(T))
						{
							ScriptPlayable<T> scriptPlayable = (ScriptPlayable<T>)rootInput;
							T trackMixer = scriptPlayable.GetBehaviour();

							if (trackMixer.GetTrackAsset() == track)
							{
								return trackMixer;
							}
						}

						//Otherwise search this playable's inputs
						{
							T trackMixer = GetTrackMixer<T>(rootInput, track);

							if (trackMixer != null)
							{
								return trackMixer;
							}
						}
					}
				}

				return null;
			}
		}
	}
}