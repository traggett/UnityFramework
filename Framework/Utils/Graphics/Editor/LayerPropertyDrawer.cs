using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(LayerProperty))]
			public class LayerPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);
					
                    if (!property.hasMultipleDifferentValues)
                    {
                        SerializedProperty layerProperty = property.FindPropertyRelative("_layer");
                        layerProperty.intValue = EditorGUI.LayerField(position, label, layerProperty.intValue);
                    }
					
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