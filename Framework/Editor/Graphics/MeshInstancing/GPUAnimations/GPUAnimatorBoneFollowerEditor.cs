using UnityEditor;

namespace Framework
{
	using Maths;

	namespace Graphics
	{
		namespace MeshInstancing
		{
			namespace GPUAnimations
			{
				namespace Editor
				{
					[CustomEditor(typeof(GPUAnimatorBoneFollower))]
					public class GPUAnimatorBoneFollowerEditor : UnityEditor.Editor
					{
						private SerializedProperty _animatorProperty;
						private SerializedProperty _boneProperty;
						private SerializedProperty _flagsProperty;

						void OnEnable()
						{
							_animatorProperty = serializedObject.FindProperty("_animator");
							_boneProperty = serializedObject.FindProperty("_boneName");
							_flagsProperty = serializedObject.FindProperty("_flags");
						}

						public override void OnInspectorGUI()
						{
							GPUAnimatorBoneFollower boneTracker = target as GPUAnimatorBoneFollower;

							EditorGUILayout.PropertyField(_animatorProperty);
							EditorGUILayout.Separator();

							//Need to draw bone name using drop down from tracked bones in bones renderer?
							EditorGUILayout.PropertyField(_boneProperty);
							EditorGUILayout.Separator();

							_flagsProperty.intValue = (int)(TransformFlags)EditorGUILayout.EnumFlagsField(_flagsProperty.displayName, (TransformFlags)_flagsProperty.intValue);

							serializedObject.ApplyModifiedProperties();
						}
					}
				}
			}
		}
	}
}