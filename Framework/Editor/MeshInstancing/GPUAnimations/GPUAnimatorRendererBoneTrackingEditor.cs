using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			namespace Editor
			{
				[CustomEditor(typeof(GPUAnimatorRendererBoneTracking), true)]
				public class GPUAnimatorRendererBoneTrackingEditor : UnityEditor.Editor
				{
					private ReorderableList _trackedBonesList;
					private GPUAnimatorRenderer _renderer;

					void OnEnable()
					{
						GPUAnimatorRendererBoneTracking boneTracker = target as GPUAnimatorRendererBoneTracking;
						
						_renderer = boneTracker.GetComponent<GPUAnimatorRenderer>();

						_trackedBonesList = new ReorderableList(new GPUAnimatorRendererBoneTracking.TrackedBone[0], typeof(GPUAnimatorRendererBoneTracking.TrackedBone), false, true, true, true)
						{
							drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawTrackedBoneHeader),
							drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawTrackedBoneItem),
							showDefaultBackground = true,
							index = 0,
							elementHeight = 20f
						};
					}

					public override void OnInspectorGUI()
					{
						GPUAnimatorRendererBoneTracking boneTracker = target as GPUAnimatorRendererBoneTracking;

						EditorGUILayout.Separator();

						//Draw TrackedBones - with bone names replaced with dropdown for bones from Animated Texture
						{
							EditorGUI.BeginChangeCheck();							
							_trackedBonesList.list = new List<GPUAnimatorRendererBoneTracking.TrackedBone>(boneTracker._trackedBones);
							_trackedBonesList.DoLayoutList();

							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(target, "Changed Tracked Bones");

								boneTracker._trackedBones = new GPUAnimatorRendererBoneTracking.TrackedBone[_trackedBonesList.list.Count];
								for (int i = 0; i < boneTracker._trackedBones.Length; i++)
									boneTracker._trackedBones[i] = (GPUAnimatorRendererBoneTracking.TrackedBone)_trackedBonesList.list[i];
							}
						}
					}

					protected virtual void OnDrawTrackedBoneHeader(Rect rect)
					{
						GUI.Label(rect, "Tracked Bones", EditorStyles.boldLabel);
					}

					private void OnDrawTrackedBoneItem(Rect rect, int index, bool selected, bool focused)
					{
						GPUAnimatorRendererBoneTracking.TrackedBone trackedBone = (GPUAnimatorRendererBoneTracking.TrackedBone)_trackedBonesList.list[index];

						GPUAnimations animations = _renderer._animationTexture.GetAnimations();

						if (animations != null)
						{
							float columnWidth = rect.width / 2f;
							rect.width = columnWidth;

							EditorGUI.LabelField(rect, new GUIContent("Bone Name"));

							string[] boneNames = animations._bones;

							if (boneNames != null && boneNames.Length > 0)
							{
								int boneIndex = 0;

								for (int i = 0; i < boneNames.Length; i++)
								{
									if (boneNames[i] == trackedBone._bone)
										boneIndex = i;
								}

								rect.x += rect.width;
								boneIndex = EditorGUI.Popup(rect, boneIndex, boneNames);
								trackedBone._bone = boneNames[boneIndex];

								_trackedBonesList.list[index] = trackedBone;
							}
						}
						else
						{
							EditorGUI.LabelField(rect, new GUIContent("Invalid GPU Animation Asset"));
						}
					}
				}
			}
		}
    }
}
