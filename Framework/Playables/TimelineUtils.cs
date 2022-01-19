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

			public static bool IsPrimaryClip(TimelineClip clip, PlayableDirector director)
			{
				//If doing pre extrapolation then is primary
				if (clip.hasPreExtrapolation && clip.extrapolatedStart <= director.time && director.time <= clip.start)
					return true;

				//If doing post extrapolation then is primary
				if (clip.hasPostExtrapolation && clip.start <= director.time && director.time <= clip.start + clip.extrapolatedDuration)
					return true;

				//if this clip is blending in, this is primary
				if (clip.hasBlendIn && clip.start <= director.time && director.time <= clip.start + clip.blendInDuration)
					return true;

				//if this clip is blending out, is not primary
				if (clip.hasBlendOut && clip.end - clip.blendOutDuration <= director.time && director.time <= clip.end)
					return false;

				//if during clip main then is primary
				if (clip.start <= director.time && director.time <= clip.end)
					return true;

				return false;
			}

			public static float GetExtrapolatedTrackTime(TimelineClip clip, double directorTime, float animationLength)
			{
				TimelineClip.ClipExtrapolation extrapolation = directorTime < clip.start ? clip.preExtrapolationMode : clip.postExtrapolationMode;
				float time = (float)(directorTime - clip.start);

				if (clip.start <= directorTime && directorTime < clip.end)
					return time;

				if (animationLength <= 0.0f)
					return 0.0f;

				switch (extrapolation)
				{
					case TimelineClip.ClipExtrapolation.Continue:
					case TimelineClip.ClipExtrapolation.Hold:
						return time < 0.0f ? 0.0f : (float)clip.end;
					case TimelineClip.ClipExtrapolation.Loop:
						{
							if (time < 0.0f)
							{
								float t = -time / animationLength;
								int n = Mathf.FloorToInt(t);
								float fraction = animationLength - (t - n);

								time = (animationLength * n) + fraction;
							}

							return time;
						}
					case TimelineClip.ClipExtrapolation.PingPong:
						{
							float t = Mathf.Abs(time) / animationLength;
							int n = Mathf.FloorToInt(t);
							float fraction = t - n;

							if (n % 2 == 1)
								fraction = animationLength - fraction;

							return (animationLength * n) + fraction;
						}
					case TimelineClip.ClipExtrapolation.None:
					default:
						return 0.0f;
				}
			}

#if UNITY_EDITOR
			public static void CreateAnimationCurves(TimelineClip clip)
			{
				FieldInfo field = clip.GetType().GetField("m_AnimationCurves", BindingFlags.NonPublic | BindingFlags.Instance);
				AnimationClip animation = CreateAnimationClipForTrack("Clip Parameters", clip.GetParentTrack(), true);
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
						: (float)timelineAsset.editorSettings.frameRate
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
		}
	}
}