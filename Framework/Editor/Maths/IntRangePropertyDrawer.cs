using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Maths
	{
		namespace Editor
		{
			[CustomPropertyDrawer(typeof(IntRange))]
			public class IntRangePropertyDrawer : RangePropertyDrawer<int>
			{
				public static IntRange FloatRangeField(Rect position, IntRange range, GUIContent label = null)
				{
					if (label != null)
					{
						Rect labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
						position = new Rect(labelPosition.x + labelPosition.width, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
						EditorGUI.LabelField(labelPosition, label);

					}
					
					int[] values = new int[] { range.Min, range.Max };
					EditorGUI.MultiIntField(position, new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);

					return new IntRange(values[0], values[1]);
				}
			}
		}
	}
}