using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Maths
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(FloatRange))]
			public class FloatRangeDrawer : RangePropertyDrawer<float>
			{
				public static FloatRange FloatRangeField(Rect position, FloatRange range, GUIContent label = null)
				{
					if (label != null)
					{
						Rect labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
						position = new Rect(labelPosition.x + labelPosition.width, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
						EditorGUI.LabelField(labelPosition, label);
					}
					
					float[] values = new float[] { range._min, range._max };
					EditorGUI.MultiFloatField(position, new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);

					return new FloatRange(range._min, range._max);
				}
			}
		}
	}
}