using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace MeshInstancing
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(AnimationTextureRef))]
			public class AnimationTextureRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					SerializedProperty assetProp = property.FindPropertyRelative("_asset");

					EditorGUI.ObjectField(position, assetProp, label);


					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return EditorGUIUtility.singleLineHeight;
				}
			}
		}
	}
}