using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace Framework
{
	using Utils.Editor;	

	namespace Playables
	{
		namespace Editor
		{
			[CustomEditor(typeof(BaseAnimatedClipAsset), true)]
			public class BaseAnimatedClipAssetInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					DrawDefaultInspector();

					EditorGUILayout.Separator();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorUtils.GetLabelWidth()));

						if (GUILayout.Button("Match Curves To Clip", GUILayout.ExpandWidth(false)))
						{
							BaseAnimatedClipAsset clipAsset = target as BaseAnimatedClipAsset;
							
							if (clipAsset != null && TimelineEditor .inspectedAsset != null)
							{
								TimelineClip clip = TimelineUtils.GetClip(TimelineEditor.inspectedAsset, clipAsset);
								MatchCurvesToClipDuration(clip);
							}
						}
					}
					EditorGUILayout.EndHorizontal();
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
					
						if (bindings.Length > 0)
						{
							//First find mix / max keyframe times
							double minKeyframeTime = double.MaxValue;
							double maxKeyframeTime = double.MinValue;

							for (int i = 0; i < bindings.Length; i++)
							{
								AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, bindings[i]);

								for (int j = 0; j < curve.keys.Length; j++)
								{
									minKeyframeTime = Math.Min(minKeyframeTime, curve.keys[j].time);
									maxKeyframeTime = Math.Max(maxKeyframeTime, curve.keys[j].time);
								}
							}

							//Work out how much to shift clip back by
							double shift = -minKeyframeTime;

							//Then work out scale
							double scale = clip.duration / (maxKeyframeTime + shift);

							//Then apply scale to all keyframes
							for (int i = 0; i < bindings.Length; i++)
							{
								AnimationCurve curve = AnimationUtility.GetEditorCurve(animation, bindings[i]);
								Keyframe[] keyframes = new Keyframe[curve.keys.Length];

								for (int j = 0; j < curve.keys.Length; j++)
								{
									keyframes[j] = curve.keys[j];
									keyframes[j].time = curve.keys[j].time + (float)shift;
									keyframes[j].time = (float)(keyframes[j].time * scale);
								}

								curve.keys = keyframes;

								if (animation != null && clip != null)
									AnimationUtility.SetEditorCurve(animation, bindings[i], curve);
							}
						}
					}
				}
#endif
			}
		}
	}
}