using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace Framework
{
	namespace Playables
	{
		public static class TimelineUtils
		{
			//Finds the TimelineClip corresponding to a playable asset in a parent track
			public static TimelineClip GetClip(TrackAsset track, IPlayableAsset Clip)
			{
				if (track != null)
				{
					IEnumerable<TimelineClip> clips = track.GetClips();

					foreach (TimelineClip clip in clips)
					{
						if (clip.asset == (Object)Clip)
							return clip;
					}
				}

				return null;
			}

			//Finds the TimelineClip corresponding to a playable asset in a timeline
			public static TimelineClip GetClip(TimelineAsset timeline, IPlayableAsset Clip)
			{
				if (timeline != null)
				{
					for (int i=0; i< timeline.rootTrackCount; i++)
					{
						TimelineClip clip = GetClipInAllTracks(timeline.GetRootTrack(i), Clip);

						if (clip != null)
							return clip;
					}
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


#if UNITY_EDITOR
			public static void CreateAnimationCurves(TimelineClip clip)
			{
				FieldInfo field = clip.GetType().GetField("m_AnimationCurves", BindingFlags.NonPublic | BindingFlags.Instance);
				AnimationClip animation = CreateAnimationClipForTrack("Clip Parameters", clip.parentTrack, true);
				field.SetValue(clip, animation);
			}

			private static AnimationClip CreateAnimationClipForTrack(string name, TrackAsset track, bool isLegacy)
			{
				var timelineAsset = track != null ? track.timelineAsset : null;
				var trackFlags = track != null ? track.hideFlags : HideFlags.None;

				var curves = new AnimationClip
				{
					legacy = isLegacy,

					name = name,

					frameRate = timelineAsset == null
						? 30f
						: timelineAsset.editorSettings.fps
				};

				SaveAssetIntoObject(curves, timelineAsset);
				curves.hideFlags = trackFlags & ~HideFlags.HideInHierarchy; // Never hide in hierarchy

				//TimelineUndo.RegisterCreatedObjectUndo(curves, "Create Curves");

				return curves;
			}

			private static void SaveAssetIntoObject(Object childAsset, Object masterAsset)
			{
				if (childAsset == null || masterAsset == null)
					return;

				if ((masterAsset.hideFlags & HideFlags.DontSave) != 0)
				{
					childAsset.hideFlags |= HideFlags.DontSave;
				}
				else
				{
					childAsset.hideFlags |= HideFlags.HideInHierarchy;
					if (!AssetDatabase.Contains(childAsset) && AssetDatabase.Contains(masterAsset))
						AssetDatabase.AddObjectToAsset(childAsset, masterAsset);
				}
			}
#endif


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

			private static TimelineClip GetClipInAllTracks(TrackAsset track, IPlayableAsset Clip)
			{
				TimelineClip clip = GetClip(track, Clip);

				if (clip != null)
					return clip;

				foreach (TrackAsset childTrack in track.GetChildTracks())
				{
					clip = GetClipInAllTracks(childTrack, Clip);

					if (clip != null)
						return clip;

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