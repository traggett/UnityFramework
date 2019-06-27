using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			namespace Editor
			{
				[CustomEditor(typeof(GPUAnimatorRendererControllerOverrider))]
				public class GPUAnimatorRendererControllerOverriderEditor : UnityEditor.Editor
				{
					private SerializedProperty _controllersProperty;
					
					void OnEnable()
					{
						_controllersProperty = serializedObject.FindProperty("_controllers");
					}

					public override void OnInspectorGUI()
					{
						GPUAnimatorRendererControllerOverrider controllerOverrider = (GPUAnimatorRendererControllerOverrider)target;
						
						if (DrawDefaultInspector())
						{
							serializedObject.ApplyModifiedProperties();
							controllerOverrider.CacheOverrideClips();
						}

						EditorGUILayout.Separator();
						
						if (GUILayout.Button("Update Animation Overides"))
						{
							controllerOverrider.CacheOverrideClips();
						}
					}

				}
			}
		}
    }
}
