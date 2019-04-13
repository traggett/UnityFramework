using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(MaterialPerInstanceProperties.Vector4Property))]
			public class MaterialPerInstanceVector4PropertyDrawer : PropertyDrawer
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

					switch ((MaterialPerInstanceProperties.Vector4Property.ePropertySource)sourceProperty.enumValueIndex)
					{
						case MaterialPerInstanceProperties.Vector4Property.ePropertySource.Constant:
							{
								SerializedProperty valueProperty = property.FindPropertyRelative("_value");
								EditorGUI.PropertyField(valueRect, valueProperty, new GUIContent("Value"));
							}
							break;
						case MaterialPerInstanceProperties.Vector4Property.ePropertySource.Range:
							{
								SerializedProperty valueXProperty = property.FindPropertyRelative("_xValueRange");
								SerializedProperty valueyProperty = property.FindPropertyRelative("_xValueRange");
								SerializedProperty valuezProperty = property.FindPropertyRelative("_xValueRange");
								SerializedProperty valuewProperty = property.FindPropertyRelative("_xValueRange");

								EditorGUI.PropertyField(valueRect, valueXProperty, new GUIContent("X Value"));
								valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + valueRect.height);
								EditorGUI.PropertyField(valueRect, valueyProperty, new GUIContent("Y Value"));
								valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + valueRect.height);
								EditorGUI.PropertyField(valueRect, valuezProperty, new GUIContent("Z Value"));
								valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + valueRect.height);
								EditorGUI.PropertyField(valueRect, valuewProperty, new GUIContent("W Value"));
							}
							break;
						case MaterialPerInstanceProperties.Vector4Property.ePropertySource.Curves:
							{
								SerializedProperty valueXProperty = property.FindPropertyRelative("_xValueCurve");
								SerializedProperty valueyProperty = property.FindPropertyRelative("_xValueCurve");
								SerializedProperty valuezProperty = property.FindPropertyRelative("_xValueCurve");
								SerializedProperty valuewProperty = property.FindPropertyRelative("_xValueCurve");

								EditorGUI.PropertyField(valueRect, valueXProperty, new GUIContent("X Value"));
								valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + valueRect.height);
								EditorGUI.PropertyField(valueRect, valueyProperty, new GUIContent("Y Value"));
								valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + valueRect.height);
								EditorGUI.PropertyField(valueRect, valuezProperty, new GUIContent("Z Value"));
								valueRect.position = new Vector2(valueRect.position.x, valueRect.position.y + valueRect.height);
								EditorGUI.PropertyField(valueRect, valuewProperty, new GUIContent("W Value"));
							}
							break;
					}

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					float height = EditorGUIUtility.singleLineHeight * 2;

					SerializedProperty sourceProperty = property.FindPropertyRelative("_source");
					MaterialPerInstanceProperties.Vector4Property.ePropertySource source = (MaterialPerInstanceProperties.Vector4Property.ePropertySource)sourceProperty.enumValueIndex;

					if (source == MaterialPerInstanceProperties.Vector4Property.ePropertySource.Range || source == MaterialPerInstanceProperties.Vector4Property.ePropertySource.Curves)
						height += EditorGUIUtility.singleLineHeight * 3;

					return height;
				}
			}
		}
	}
}