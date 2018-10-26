using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace AnimationSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(IAnimatorRefProperty))]
			public class IAnimatorRefPropertyDrawer : PropertyDrawer
			{
				public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
				{
					EditorGUI.BeginProperty(position, label, property);

					SerializedProperty componentProp = property.FindPropertyRelative("_animator");
					SerializedProperty editorHeightProp = property.FindPropertyRelative("_editorHeight");
					
					EditorGUI.BeginChangeCheck();
					
					float height;
					Component currentComponent = componentProp.objectReferenceValue as Component;
					Component component = EditorUtils.ComponentField<IAnimator>(label, position, currentComponent, out height);
					if (component != currentComponent)
						componentProp.objectReferenceValue = component;

					editorHeightProp.floatValue = height;

					EditorGUI.EndProperty();
				}

				public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
				{
					SerializedProperty editorHeightProp = property.FindPropertyRelative("_editorHeight");
					return editorHeightProp.floatValue;
				}
			}
		}
	}
}