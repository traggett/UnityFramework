using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicComponentRef))]
			public class DynamicComponentRefPropertyDrawer : DynamicValuePropertyDrawer<Component>
			{
				public override void DrawStaticValueField(Rect position, SerializedProperty valueProperty)
				{
					Component currentComponent = valueProperty.objectReferenceValue as Component;
					float height;
					Component component = EditorUtils.ComponentField<Component>(new GUIContent("Component"), position, currentComponent, out height);
					if (component != currentComponent)
						valueProperty.objectReferenceValue = component;
				}

				public override float GetStaticValueFieldHeight(SerializedProperty valueProperty)
				{
					Component currentComponent = valueProperty.objectReferenceValue as Component;
					return EditorUtils.GetComponentFieldHeight<Component>(currentComponent);
				}
			}
		}
	}
}