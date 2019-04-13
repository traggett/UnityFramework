using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(MaterialPerInstanceProperties.TextureProperty))]
			public class MaterialPerInstanceTexturePropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty nameProperty = property.FindPropertyRelative("_name");
					SerializedProperty sourceProperty = property.FindPropertyRelative("_source");

					Rect nameRect = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
					Rect sourceRect = new Rect(position.x + nameRect.width, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

					EditorGUI.PropertyField(nameRect, nameProperty, new GUIContent(""));
					EditorGUI.PropertyField(sourceRect, sourceProperty, new GUIContent(""));
					
					Rect valueRect = new Rect(position.x, position.y + nameRect.height, position.width, EditorGUIUtility.singleLineHeight);

					switch ((MaterialPerInstanceProperties.TextureProperty.ePropertySource)sourceProperty.enumValueIndex)
					{
						case MaterialPerInstanceProperties.TextureProperty.ePropertySource.Constant:
							{
								SerializedProperty valueProperty = property.FindPropertyRelative("_value");
								EditorGUI.PropertyField(valueRect, valueProperty, new GUIContent("Value"));
							}
							break;
						case MaterialPerInstanceProperties.TextureProperty.ePropertySource.Array:
							{
								SerializedProperty valueProperty = property.FindPropertyRelative("_valueArray");
								EditorGUI.PropertyField(valueRect, valueProperty, new GUIContent("Value"), true);
							}
							break;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					return EditorGUIUtility.singleLineHeight * 2;
				}
			}
		}
	}
}