using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
    namespace Animations
    {
        namespace Editor
        {
            [CustomPropertyDrawer(typeof(AnimatorLODControllerOverrides.AnimatorData))]
            public class AnimatorLODDataPropertyDrawer : PropertyDrawer
            {
                #region PropertyDrawer
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    EditorGUI.BeginProperty(position, label, property);

                    SerializedProperty controllerProp = property.FindPropertyRelative("_controller");
                    SerializedProperty animatorLODsProp = property.FindPropertyRelative("_animatorLODs");

                    Rect controllerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
                    EditorGUI.PropertyField(controllerRect, controllerProp, new GUIContent("Animator Controller"));

                    Rect lodsRect = new Rect(position.x + EditorGUIUtility.singleLineHeight, controllerRect.y + controllerRect.height, position.width - EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
                    animatorLODsProp.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(lodsRect, animatorLODsProp.isExpanded, "LODs");
                    if (animatorLODsProp.isExpanded)
                    {
                        EditorGUI.indentLevel++;
                        float labelWidth = EditorGUIUtility.labelWidth;
                        EditorGUIUtility.labelWidth = labelWidth * 0.5f;

                        float deleteButtonWidth = EditorGUIUtility.singleLineHeight * 2f;
                        float deleteButtonSpacing = EditorGUIUtility.singleLineHeight;
                        float elementPropWidth = lodsRect.width - deleteButtonWidth - deleteButtonSpacing;
                        Rect elementRect = new Rect(lodsRect.x, lodsRect.y + lodsRect.height, elementPropWidth, EditorGUIUtility.singleLineHeight);

                        //Draw LODS
                        for (int i = 0; i < animatorLODsProp.arraySize;)
                        {
                            SerializedProperty minLODLevelProp = property.FindPropertyRelative("_animatorLODs.Array.data[" + i + "]._minLODLevel");
                            SerializedProperty avatarMaskProp = property.FindPropertyRelative("_animatorLODs.Array.data[" + i + "]._avatarMask");

                            Rect lodLevelRect = new Rect(elementRect.x, elementRect.y, elementRect.width * 0.5f, elementRect.height);
                            minLODLevelProp.intValue = EditorGUI.IntSlider(lodLevelRect, new GUIContent("Min LOD Level"), minLODLevelProp.intValue, 1, 8);

                            Rect maskRect = new Rect(lodLevelRect.x + lodLevelRect.width, elementRect.y, elementRect.width - lodLevelRect.width, elementRect.height);
                            avatarMaskProp.objectReferenceValue = (AvatarMask)EditorGUI.ObjectField(maskRect, new GUIContent("LOD Bone Mask"), avatarMaskProp.objectReferenceValue, typeof(AvatarMask), false);

                            Rect deleteElementRect = new Rect(maskRect.x + maskRect.width + deleteButtonSpacing, elementRect.y, deleteButtonWidth, elementRect.height);
                            if (GUI.Button(deleteElementRect, "-"))
                            {
                                animatorLODsProp.DeleteArrayElementAtIndex(i);
                            }
                            else
                            {
                                i++;
                            }

                            elementRect.y += EditorGUIUtility.singleLineHeight;
                        }

                        Rect addElementRect = new Rect(elementRect.x + elementRect.width + deleteButtonSpacing, elementRect.y, deleteButtonWidth, elementRect.height);
                        if (GUI.Button(addElementRect, "+"))
                        {
                            animatorLODsProp.InsertArrayElementAtIndex(animatorLODsProp.arraySize);
                        }

                        EditorGUI.indentLevel--;
                        EditorGUIUtility.labelWidth = labelWidth;
                    }
                    EditorGUI.EndFoldoutHeaderGroup();

                    EditorGUI.EndProperty();
                }

                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    SerializedProperty animatorLODsProp = property.FindPropertyRelative("_animatorLODs");

                    return EditorGUIUtility.singleLineHeight * (2 + (animatorLODsProp.isExpanded ? (animatorLODsProp.arraySize + 1) : 0));
                }
                #endregion
            }

            [CustomEditor(typeof(AnimatorLODControllerOverrides))]
            public class AnimatorLODControllerOverridesEditor : UnityEditor.Editor
            {
                #region Private Data
                private ReorderableList _dataList;
                #endregion

                #region UnityEditor.Editor
                public void OnEnable()
                {
                    _dataList = new ReorderableList(serializedObject, serializedObject.FindProperty("_animatorData"), false, false, true, true)
                    {
                        drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawElement),
                        elementHeightCallback = new ReorderableList.ElementHeightCallbackDelegate(OnGetElementHeight),
                    };
                }

                public override void OnInspectorGUI()
                {
                    AnimatorLODControllerOverrides controllerOverrides = target as AnimatorLODControllerOverrides;
                    if (controllerOverrides == null)
                        return;

                    EditorGUI.BeginChangeCheck();

                    _dataList.index = -1;
                    _dataList.DoLayoutList();

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        CacheOverrideClips();
                    }

                    EditorGUILayout.Separator();

                    if (GUILayout.Button("Update Animation Overides"))
                    {
                        CacheOverrideClips();
                    }
                }
                #endregion

                #region ReorderableList Callbacks
                private float OnGetElementHeight(int index)
                {
                    SerializedProperty property = serializedObject.FindProperty("_animatorData.Array.data[" + index + "]");
                    return EditorGUI.GetPropertyHeight(property);
                }

                private void OnDrawElement(Rect rect, int index, bool selected, bool focused)
                {
                    SerializedProperty property = serializedObject.FindProperty("_animatorData.Array.data[" + index + "]");
                    EditorGUI.PropertyField(rect, property);
                }
                #endregion

                #region Private Functions
                private void CacheOverrideClips(RuntimeAnimatorController controller, ref AnimatorLODControllerOverrides.AnimatorLOD animatorLOD)
                {
                    //Use mask to work out what bones to keep
                    List<AnimationClip> clips = new List<AnimationClip>();

                    foreach (AnimationClip clip in controller.animationClips)
                    {
                        if (clip != null && !clips.Contains(clip))
                        {
                            clips.Add(clip);
                        }
                    }

                    animatorLOD._overrideClips = new AnimatorLODControllerOverrides.AnimationClipData[clips.Count];

                    for (int i = 0; i < animatorLOD._overrideClips.Length; i++)
                    {
                        animatorLOD._overrideClips[i] = new AnimatorLODControllerOverrides.AnimationClipData()
                        {
                            _originalClip = clips[i],
                            _overrideClip = CreateOverrideClip(clips[i], animatorLOD._avatarMask)
                        };
                    }
                }

                private void CacheOverrideClips()
                {
                    AnimatorLODControllerOverrides controllerOverrides = (AnimatorLODControllerOverrides)target;

                    for (int i = 0; i < controllerOverrides._animatorData.Length; i++)
                    {
                        for (int j = 0; j < controllerOverrides._animatorData[i]._animatorLODs.Length; j++)
                        {
                            CacheOverrideClips(controllerOverrides._animatorData[i]._controller, ref controllerOverrides._animatorData[i]._animatorLODs[j]);
                        }
                    }

                    EditorSceneManager.MarkSceneDirty(controllerOverrides.gameObject.scene);
                }

                private static AnimationClip CreateOverrideClip(AnimationClip origClip, AvatarMask avatarMask)
                {
                    AnimationClip overrideClip = new AnimationClip
                    {
                        name = origClip.name,
                        wrapMode = origClip.wrapMode,
                        legacy = true,
                        frameRate = origClip.frameRate,
                        localBounds = origClip.localBounds,
                    };

                    AnimationUtility.SetAnimationEvents(overrideClip, origClip.events);

                    overrideClip.SetCurve("", typeof(AnimatorLODGroup), "_animatedValue", new AnimationCurve(new Keyframe(origClip.length, 0f)));

                    //need to also add curves contained in avatar maask
                    if (avatarMask != null)
                    {
                        for (int i = 0; i < avatarMask.transformCount; i++)
                        {
                            if (avatarMask.GetTransformActive(i))
                            {
                                AddTransformCurves(origClip, overrideClip, avatarMask.GetTransformPath(i));
                            }
                        }
                    }

                    overrideClip.legacy = false;

                    return overrideClip;
                }

                private static void AddTransformCurves(AnimationClip origClip, AnimationClip clip, string relativePath)
                {
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalRotation.x", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalRotation.y", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalRotation.z", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalRotation.w", typeof(Transform));

                    AddTransformCurve(origClip, clip, relativePath, "m_LocalScale.x", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalScale.y", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalScale.z", typeof(Transform)); ;

                    AddTransformCurve(origClip, clip, relativePath, "m_LocalPosition.x", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalPosition.y", typeof(Transform));
                    AddTransformCurve(origClip, clip, relativePath, "m_LocalPosition.z", typeof(Transform)); ;
                }

                private static void AddTransformCurve(AnimationClip origClip, AnimationClip clip, string relativePath, string propertyName, Type type)
                {
                    AnimationCurve curve = AnimationUtility.GetEditorCurve(origClip, EditorCurveBinding.FloatCurve(relativePath, type, propertyName));

                    if (curve != null)
                    {
                        clip.SetCurve(relativePath, type, propertyName, curve);
                    }
                }
                #endregion
            }
        }
    }
}
