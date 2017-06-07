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
				public override float DrawValueField(Rect position, SerializedProperty valueProperty)
				{
					MaterialRefPropertyDrawer materialRefPropertyDrawer = new MaterialRefPropertyDrawer();
					GUIContent label = new GUIContent("Material");

					position.height = materialRefPropertyDrawer.GetPropertyHeight(valueProperty, label);
					materialRefPropertyDrawer.OnGUI(position, valueProperty, label);
					
					return position.height;
				}
			}
		}
	}
}