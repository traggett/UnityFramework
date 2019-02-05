using System;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace Playables
	{
		namespace Editor
		{
			[CustomEditor(typeof(ParentBindingMasterClip), true)]
			public class ParentBindingMasterClipInspector : UnityEditor.Editor
			{
				public override void OnInspectorGUI()
				{
					
					DrawDefaultInspector();
					
					EditorGUILayout.Separator();

					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField(GUIContent.none, GUILayout.Width(EditorUtils.GetLabelWidth()));

						if (GUILayout.Button("Set Duration From Child Tracks", GUILayout.ExpandWidth(false)))
						{
							ParentBindingMasterClip Clip = target as ParentBindingMasterClip;

							if (Clip != null && TimelineEditor.inspectedAsset != null)
							{
								TimelineClip clip = TimelineUtils.GetClip(TimelineEditor.inspectedAsset, Clip);
								ClampMasterClipToChildClips(clip);
							}
						}
					}
					EditorGUILayout.EndHorizontal();
				}

				public void ClampMasterClipToChildClips(TimelineClip masterClip)
				{
					if (masterClip != null)
					{
						double startTime = double.MaxValue;
						double endTime = double.MinValue;
						bool hasClips = false;

						foreach (TrackAsset child in masterClip.parentTrack.GetChildTracks())
						{
							foreach (TimelineClip clip in child.GetClips())
							{
								double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
								double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

								startTime = Math.Min(startTime, clipStart);
								endTime = Math.Max(endTime, clipStart + clipDuration);
								hasClips = true;
							}
						}

						if (hasClips)
						{
							masterClip.start = startTime;
							masterClip.duration = endTime - startTime;
						}
					}
				}
			}
		}
	}
}