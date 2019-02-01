using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using System;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Playables
	{
		public abstract class AnimatedTrack : TrackAsset
		{

#if UNITY_EDITOR
			protected override void OnCreateClip(TimelineClip clip)
			{
				AnimatedClipAsset animatedClip = clip.asset as AnimatedClipAsset;

				if (animatedClip != null)
				{
					animatedClip._cachedDuration = clip.duration;
					clip.CreateCurves("Clip Parameters");
					clip.curves.ClearCurves();
					animatedClip.AddDefaultCurves(clip);
				}
			}

			protected override void OnBeforeTrackSerialize()
			{
				base.OnBeforeTrackSerialize();

				CheckTrackDurations();
			}

			private void CheckTrackDurations()
			{
				if (Application.isEditor)
				{
					//Check any animation clips have been resized?
					IEnumerable<TimelineClip> clips = GetClips();
					bool changedClip = false;

					foreach (TimelineClip clip in clips)
					{
						AnimatedClipAsset animatedClip = clip.asset as AnimatedClipAsset;

						if (animatedClip != null && animatedClip._cachedDuration != clip.duration)
						{
							animatedClip._cachedDuration = clip.duration;
							MatchCurvesToClipDuration(clip);
							
							changedClip = true;
						}
					}

					if (changedClip)
					{
						EditorUtility.SetDirty(this.timelineAsset);
						AssetDatabase.Refresh();
					}
				}
			}

			private void MatchCurvesToClipDuration(TimelineClip clip)
			{
				if (clip == null)
					return;

				AnimatedClipAsset animatedClip = clip.asset as AnimatedClipAsset;
				AnimationClip animation = clip.curves;

				if (animation != null)
				{
					EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(animation);

					//First find max keyframe time
					double maxKeyframeTime = 0d;

					for (int i=0; i < bindings.Length; i++)
					{
						AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, bindings[i]);

						for (int j = 0; j < curve.keys.Length; j++)
						{
							maxKeyframeTime = Math.Max(maxKeyframeTime, curve.keys[j].time);
						}
					}

					//Then work out scale
					double scale = clip.duration / maxKeyframeTime;

					//Then apply scale to all keyframes
					for (int i = 0; i < bindings.Length; i++)
					{
						AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, bindings[i]);
						Keyframe[] keyframes = new Keyframe[curve.keys.Length];

						for (int j = 0; j < curve.keys.Length; j++)
						{
							keyframes[j] = curve.keys[j];
							keyframes[j].time = (float)(curve.keys[j].time * scale);
						}

						curve.keys = keyframes;

						if (animation != null && clip != null)
							AnimationUtility.SetEditorCurve(animation, bindings[i], curve);
					}
				}
			}
#endif


		}
	}
}