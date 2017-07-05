using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(ComponentValueSource))]
			public class ComponentValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Component>
			{
				public override void DrawValueField(Rect position, SerializedProperty valueProperty)
				{
					Component currentComponent = valueProperty.objectReferenceValue as Component;
					float height;
					Component component = EditorUtils.ComponentField<Component>(new GUIContent("Component"), position, currentComponent, out height);
					if (component != currentComponent)
						valueProperty.objectReferenceValue = component;
				}

				public override float GetValueFieldHeight(SerializedProperty valueProperty)
				{
					return base.GetValueFieldHeight(valueProperty);
				}
			}
		}
	}
}