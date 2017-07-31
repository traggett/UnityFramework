using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace DynamicValueSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(DynamicMaterialRef))]
			public class DynamicMaterialRefPropertyDrawer : DynamicValuePropertyDrawer<Material>
			{
				public override void DrawStaticValueField(Rect position, SerializedProperty valueProperty)
				{
					MaterialRefPropertyDrawer materialRefPropertyDrawer = new MaterialRefPropertyDrawer();
					GUIContent label = new GUIContent("Material");

					position.height = materialRefPropertyDrawer.GetPropertyHeight(valueProperty, label);
					materialRefPropertyDrawer.OnGUI(position, valueProperty, label);
				}

				public override float GetStaticValueFieldHeight(SerializedProperty valueProperty)
				{
					MaterialRefPropertyDrawer materialRefPropertyDrawer = new MaterialRefPropertyDrawer();
					GUIContent label = new GUIContent("Material");
					return materialRefPropertyDrawer.GetPropertyHeight(valueProperty, label);
				}

				protected override bool AllowDraggingComponents()
				{
					return false;
				}
			}
		}
	}
}