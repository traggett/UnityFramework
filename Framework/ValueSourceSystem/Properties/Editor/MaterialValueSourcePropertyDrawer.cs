using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils.Editor;

	namespace ValueSourceSystem
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(MaterialValueSource))]
			public class MaterialValueSourcePropertyDrawer : ValueSourcePropertyDrawer<Material>
			{
				public override void DrawValueField(Rect position, SerializedProperty valueProperty)
				{
					MaterialRefPropertyDrawer materialRefPropertyDrawer = new MaterialRefPropertyDrawer();
					GUIContent label = new GUIContent("Material");

					position.height = materialRefPropertyDrawer.GetPropertyHeight(valueProperty, label);
					materialRefPropertyDrawer.OnGUI(position, valueProperty, label);
				}

				public override float GetValueFieldHeight(SerializedProperty valueProperty)
				{
					MaterialRefPropertyDrawer materialRefPropertyDrawer = new MaterialRefPropertyDrawer();
					GUIContent label = new GUIContent("Material");
					return materialRefPropertyDrawer.GetPropertyHeight(valueProperty, label);
				}
			}
		}
	}
}