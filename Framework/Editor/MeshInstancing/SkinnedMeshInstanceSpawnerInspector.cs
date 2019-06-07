using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		using GPUAnimations;

		namespace Editor
		{
			[CustomEditor(typeof(SkinnedMeshInstanceSpawner), true)]
			public class SkinnedMeshInstanceSpawnerInspector : UnityEditor.Editor
			{
				private ReorderableList _animationList;

				void OnEnable()
				{
					_animationList = new ReorderableList(new SkinnedMeshInstanceSpawner.AnimationData[0], typeof(SkinnedMeshInstanceSpawner.AnimationData), false, true, true, true)
					{
						drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawHeader),
						drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawAnimationItem),
						showDefaultBackground = true,
						index = 0,
						elementHeight = 20f
					};
				}

				public override void OnInspectorGUI()
				{
					base.DrawDefaultInspector();

					SkinnedMeshInstanceSpawner spawner = target as SkinnedMeshInstanceSpawner;

					if (spawner._animations == null)
						spawner._animations = new SkinnedMeshInstanceSpawner.AnimationData[0];

					EditorGUI.BeginChangeCheck();

					GUILayout.Label("Animations", EditorStyles.boldLabel);
					GUILayout.Space(3f);
					_animationList.list = new List<SkinnedMeshInstanceSpawner.AnimationData>(spawner._animations);
					_animationList.DoLayoutList();

					if (EditorGUI.EndChangeCheck())
					{
						Undo.RecordObject(target, "Changed Animations");

						spawner._animations = new SkinnedMeshInstanceSpawner.AnimationData[_animationList.list.Count];
						for (int i = 0; i < spawner._animations.Length; i++)
							spawner._animations[i] = (SkinnedMeshInstanceSpawner.AnimationData)_animationList.list[i];
					}
				}

				protected virtual void OnDrawHeader(Rect rect)
				{
					float columnWidth = rect.width /= 3f;
					GUI.Label(rect, "Animation", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Probability", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Speed Range", EditorStyles.label);
				}

				private void OnDrawAnimationItem(Rect rect, int index, bool selected, bool focused)
				{
					SkinnedMeshInstanceSpawner spawner = target as SkinnedMeshInstanceSpawner;
					SkinnedMeshInstanceSpawner.AnimationData animation = (SkinnedMeshInstanceSpawner.AnimationData)_animationList.list[index];
					
					float columnWidth = rect.width / 3f;
					rect.width = columnWidth;

					if (spawner._animationTexture.IsValid())
					{
						GPUAnimations.GPUAnimations animations = spawner._animationTexture.GetAnimations();

						GUIContent[] animNames = new GUIContent[animations._animations.Length];
						for (int i = 0; i < animNames.Length; i++)
							animNames[i] = new GUIContent(animations._animations[i]._name);

						animation._animationIndex = EditorGUI.Popup(rect, animation._animationIndex, animNames);
					}
					else
					{
						EditorGUI.LabelField(rect, "(Invalid Animation Texture)");
					}

					rect.x += columnWidth;
					animation._probability = EditorGUI.Slider(rect, animation._probability, 0f, 1f);

					rect.x += columnWidth;
					{
						rect.width = columnWidth * 0.5f;
						animation._speedRange._min = EditorGUI.FloatField(rect, animation._speedRange._min);
						rect.x += rect.width;
						animation._speedRange._max = EditorGUI.FloatField(rect, animation._speedRange._max);
					}
					
					_animationList.list[index] = animation;
				}
			}
		}
	}
}
