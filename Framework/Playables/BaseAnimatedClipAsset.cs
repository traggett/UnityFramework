using System;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace Playables
	{
		public abstract class BaseAnimatedClipAsset : PlayableAsset, IClipInitialiser
		{
			[NonSerialized]
			public double _cachedDuration;
			[NonSerialized]
			public TimelineClip _timelineClip;

			public abstract void AddDefaultCurves(TimelineClip clip);

			public void OnClipCreated(TimelineClip clip)
			{
				_timelineClip = clip;
				_cachedDuration = clip.duration;
				clip.CreateCurves("Clip Parameters");
				clip.curves.ClearCurves();
				AddDefaultCurves(clip);
			}

#if UNITY_EDITOR
			private void MatchCurvesToClipDuration(TimelineClip clip)
			{
				if (clip == null)
					return;

				BaseAnimatedClipAsset animatedClip = clip.asset as BaseAnimatedClipAsset;
				AnimationClip animation = clip.curves;

				if (animation != null)
				{
					EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(animation);

					//First find max keyframe time
					double maxKeyframeTime = 0d;

					for (int i = 0; i < bindings.Length; i++)
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