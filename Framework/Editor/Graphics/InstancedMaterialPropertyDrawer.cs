using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Graphics
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(InstancedMaterial))]
			public class InstancedMaterialPropertyDrawer : PropertyDrawer
			{				
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
					EditorGUI.PropertyField(position, property.FindPropertyRelative("_material"), label);
					
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