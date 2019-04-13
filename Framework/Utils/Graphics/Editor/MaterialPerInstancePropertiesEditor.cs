using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		[CustomPropertyDrawer(typeof(MaterialPerInstanceProperties.Property))]
		public class MaterialPerInstancePropertyDrawer : PropertyDrawer
		{
			public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
			{
				EditorGUI.BeginProperty(position, label, property);

				SerializedProperty typeProperty = property.FindPropertyRelative("_type");
				SerializedProperty nameProperty = property.FindPropertyRelative("_name");

				Rect namePosition = new Rect(position.x, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
				nameProperty.stringValue = EditorGUI.TextField(namePosition, nameProperty.stringValue);

				Rect typePosition = new Rect(position.x + namePosition.width, position.y, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
				typeProperty.enumValueIndex = (int)(MaterialPerInstanceProperties.Property.eType)EditorGUI.EnumPopup(typePosition, (MaterialPerInstanceProperties.Property.eType)typeProperty.enumValueIndex);

				Rect valueSourcePosition = new Rect(position.x, position.y + namePosition.height, position.width * 0.5f, EditorGUIUtility.singleLineHeight);
				Rect valuePosition = new Rect(position.x + namePosition.width, position.y + namePosition.height, position.width * 0.5f, EditorGUIUtility.singleLineHeight);

				switch ((MaterialPerInstanceProperties.Property.eType)typeProperty.enumValueIndex)
				{
					case MaterialPerInstanceProperties.Property.eType.Color:
						{
							SerializedProperty valueProperty = property.FindPropertyRelative("_vectorValue");
							valueProperty.vector4Value = EditorGUI.ColorField(valuePosition, valueProperty.vector4Value);
						}
						break;
					case MaterialPerInstanceProperties.Property.eType.Float:
						{
							SerializedProperty valueProperty = property.FindPropertyRelative("_floatValue");
							valueProperty.floatValue = EditorGUI.FloatField(valuePosition, valueProperty.floatValue);
						}
						break;
					case MaterialPerInstanceProperties.Property.eType.Int:
						{
							SerializedProperty valueProperty = property.FindPropertyRelative("_intValue");
							valueProperty.intValue = EditorGUI.IntField(valuePosition, valueProperty.intValue);
						}
						break;
					case MaterialPerInstanceProperties.Property.eType.Vector:
						{
							SerializedProperty valueProperty = property.FindPropertyRelative("_vectorValue");
							valueProperty.vector4Value = EditorGUI.Vector4Field(valuePosition, "", valueProperty.vector4Value);
						}
						break;
					case MaterialPerInstanceProperties.Property.eType.Texture:
						{
							SerializedProperty valueProperty = property.FindPropertyRelative("_textureValue");
							valueProperty.objectReferenceValue = EditorGUI.ObjectField(valuePosition, valueProperty.objectReferenceValue, typeof(Texture), false);
						}
						break;
				}

				EditorGUI.EndProperty();
			}

			public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
			{
				float height = EditorGUIUtility.singleLineHeight * 2;
				return height;
			}
		}
	}
}