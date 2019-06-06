using UnityEditor;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			namespace Editor
			{
				[CustomEditor(typeof(GPUAnimatorBoneFollower), true)]
				public class GPUAnimatorBoneFollowerEditor : UnityEditor.Editor
				{
					private SerializedProperty _boneProperty;
					private SerializedProperty _targetTransformProperty;

					void OnEnable()
					{
						_boneProperty = serializedObject.FindProperty("_boneName");
						_targetTransformProperty = serializedObject.FindProperty("_targetTransform");
					}

					public override void OnInspectorGUI()
					{
						GPUAnimatorBoneFollower boneTracker = target as GPUAnimatorBoneFollower;

						//Need to draw bone name using drop down from tracked bones in bones renderer?
						EditorGUILayout.PropertyField(_boneProperty);
						EditorGUILayout.Separator();

						//Draw Target Transform
						EditorGUILayout.PropertyField(_targetTransformProperty);
						EditorGUILayout.Separator();
						
						serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}
    }
}
