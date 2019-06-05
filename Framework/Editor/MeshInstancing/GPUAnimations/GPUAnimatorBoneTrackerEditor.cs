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
				[CustomEditor(typeof(GPUAnimatorBoneTracker), true)]
				public class GPUAnimatorBoneTrackerEditor : UnityEditor.Editor
				{
					private ReorderableList _trackedBonesList;
					private ReorderableList _boneFollowersList;
					private SerializedProperty _meshProperty;
					private GPUAnimator _animator;

					void OnEnable()
					{
						GPUAnimatorBoneTracker boneTracker = target as GPUAnimatorBoneTracker;

						_meshProperty = serializedObject.FindProperty("_referenceMesh");

						_animator = boneTracker.GetComponent<GPUAnimator>();

						_trackedBonesList = new ReorderableList(new GPUAnimatorBoneTracker.TrackedBone[0], typeof(GPUAnimatorBoneTracker.TrackedBone), false, true, true, true)
						{
							drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawTrackedBoneHeader),
							drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawTrackedBoneItem),
							showDefaultBackground = true,
							index = 0,
							elementHeight = 20f
						};

						_boneFollowersList = new ReorderableList(new GPUAnimatorBoneTracker.BoneFollower[0], typeof(GPUAnimatorBoneTracker.BoneFollower), false, true, true, true)
						{
							drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawBoneFollowerHeader),
							drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawBoneFollowerItem),
							showDefaultBackground = true,
							index = 0,
							elementHeight = 20f
						};
					}

					public override void OnInspectorGUI()
					{
						GPUAnimatorBoneTracker boneTracker = target as GPUAnimatorBoneTracker;

						//Draw _referenceMesh
						EditorGUILayout.PropertyField(_meshProperty);
						EditorGUILayout.Separator();

						//Draw TrackedBones - with bone names replaced with dropdown for bones from Animated Texture
						{
							EditorGUI.BeginChangeCheck();							
							_trackedBonesList.list = new List<GPUAnimatorBoneTracker.TrackedBone>(boneTracker._trackedBones);
							_trackedBonesList.DoLayoutList();

							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(target, "Changed Tracked Bones");

								boneTracker._trackedBones = new GPUAnimatorBoneTracker.TrackedBone[_trackedBonesList.list.Count];
								for (int i = 0; i < boneTracker._trackedBones.Length; i++)
									boneTracker._trackedBones[i] = (GPUAnimatorBoneTracker.TrackedBone)_trackedBonesList.list[i];
							}
						}

						//Draw _boneFollowers (only when have tracked bones, show index as drop down of tracked bones)
						{
							EditorGUI.BeginChangeCheck();
							_boneFollowersList.list = new List<GPUAnimatorBoneTracker.BoneFollower>(boneTracker._boneFollowers);
							_boneFollowersList.DoLayoutList();

							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(target, "Changed Tracked Bones");

								boneTracker._boneFollowers = new GPUAnimatorBoneTracker.BoneFollower[_boneFollowersList.list.Count];
								for (int i = 0; i < boneTracker._boneFollowers.Length; i++)
									boneTracker._boneFollowers[i] = (GPUAnimatorBoneTracker.BoneFollower)_boneFollowersList.list[i];
							}
						}

						serializedObject.ApplyModifiedProperties();
					}

					protected virtual void OnDrawTrackedBoneHeader(Rect rect)
					{
						GUI.Label(rect, "Tracked Bones", EditorStyles.boldLabel);
					}

					private void OnDrawTrackedBoneItem(Rect rect, int index, bool selected, bool focused)
					{
						GPUAnimatorBoneTracker.TrackedBone trackedBone = (GPUAnimatorBoneTracker.TrackedBone)_trackedBonesList.list[index];

						float columnWidth = rect.width / 2f;
						rect.width = columnWidth;


						EditorGUI.LabelField(rect, new GUIContent("Bone Name"));

						string[] boneNames = _animator._animations.GetBoneNames();

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

					protected virtual void OnDrawBoneFollowerHeader(Rect rect)
					{
						GUI.Label(rect, "Bone Followers", EditorStyles.boldLabel);
					}

					private void OnDrawBoneFollowerItem(Rect rect, int index, bool selected, bool focused)
					{
						GPUAnimatorBoneTracker.BoneFollower boneFollower = (GPUAnimatorBoneTracker.BoneFollower)_boneFollowersList.list[index];
						GPUAnimatorBoneTracker boneTracker = target as GPUAnimatorBoneTracker;

						float columnWidth = rect.width / 2f;
						rect.width = columnWidth;

						if (boneTracker._trackedBones.Length > 0)
						{
							string[] trackedBoneNames = new string[boneTracker._trackedBones.Length];

							for (int i = 0; i < trackedBoneNames.Length; i++)
								trackedBoneNames[i] = boneTracker._trackedBones[i]._bone;

							boneFollower._trackedBoneIndex = EditorGUI.Popup(rect, boneFollower._trackedBoneIndex, trackedBoneNames);
						}

						rect.x += rect.width;
						boneFollower._transform = (Transform)EditorGUI.ObjectField(rect, boneFollower._transform, typeof(Transform), true);

						_boneFollowersList.list[index] = boneFollower;
					}
				}
			}
		}
    }
}
