using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
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
					if (clip.asset == (UnityEngine.Object)clipAsset)
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

			public static PlayableBehaviour GetTrackMixer(PlayableGraph graph, TrackAsset track, Type type)
			{
				int rootCount = graph.GetRootPlayableCount();

				for (int i = 0; i < rootCount; i++)
				{
					Playable root = graph.GetRootPlayable(i);

					PlayableBehaviour trackMixer = GetTrackMixer(root, track, type);

					if (trackMixer != null)
					{
						return trackMixer;
					}
				}

				return null;
			}

			private static T GetTrackMixer<T>(Playable root, TrackAsset track) where T : class, IPlayableBehaviour, ITrackMixer, new()
			{
				int inputCount = root.GetOutputCount();

				for (int i = 0; i < inputCount; i++)
				{
					Playable rootInput = root.GetOutput(i);

					if (rootInput.IsValid())
					{
						//If this input is a T, check it matches our track
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

			private static PlayableBehaviour GetTrackMixer(Playable root, TrackAsset track, Type type)
			{
				int inputCount = root.GetOutputCount();

				for (int i = 0; i < inputCount; i++)
				{
					Playable rootInput = root.GetOutput(i);

					if (rootInput.IsValid())
					{
						//If this input is a T, check it matches our track
						if (rootInput.GetPlayableType() is ITrackMixer)
						{
							PlayableBehaviour playableBehaviour = GetPlayableBehaviour(rootInput, type);
							ITrackMixer trackMixer = playableBehaviour as ITrackMixer;

							if (trackMixer != null && trackMixer.GetTrackAsset() == track)
							{
								return playableBehaviour;
							}
						}

						//Otherwise search this playable's inputs
						{
							PlayableBehaviour trackMixer = GetTrackMixer(rootInput, track, type);

							if (trackMixer != null)
							{
								return trackMixer;
							}
						}
					}
				}

				return null;
			}

			//Get all playable behaviours in a playable graph of type T, or implementing interface of type T
			public static List<T> GetPlayableBehaviours<T>(PlayableGraph graph) where T : class
			{
				List<T> playables = new List<T>();

				int rootCount = graph.GetRootPlayableCount();

				for (int i = 0; i < rootCount; i++)
				{
					Playable root = graph.GetRootPlayable(i);

					int inputCount = root.GetInputCount(); ;

					for (int j = 0; j < inputCount; j++)
					{
						GetPlayableBehaviours(root.GetInput(j), ref playables);
					}
				}

				return playables;
			}

			private static PlayableBehaviour GetPlayableBehaviour(Playable playable, Type playableType)
			{
				Type scriptPlayableType = typeof(ScriptPlayable<>).MakeGenericType(new[] { playableType });

				MethodInfo castMethod = scriptPlayableType.GetMethod("op_Explicit", new Type[] { typeof(Playable) });

				if (castMethod != null)
				{
					object scriptPlayable = castMethod.Invoke(null, new object[] { playable });

					MethodInfo method = scriptPlayableType.GetMethod("GetBehaviour");

					if (method != null)
					{
						return method.Invoke(scriptPlayable, new object[0]) as PlayableBehaviour;
					}
				}

				return null;
			}


			private static void GetPlayableBehaviours<T>(Playable root, ref List<T> playables) where T : class
			{
				int inputCount = root.GetInputCount();

				for (int i = 0; i < inputCount; i++)
				{
					Playable node = root.GetInput(i);

					if (node.IsValid())
					{
						Type playableType = node.GetPlayableType();

						if (SystemUtils.IsTypeOf(typeof(T), playableType))
						{
							T playable = GetPlayableBehaviour(node, playableType) as T;

							if (playable != null)
							{
								playables.Add(playable);
							}
						}

						GetPlayableBehaviours(node, ref playables);
					}
				}
			}
		}
	}
}