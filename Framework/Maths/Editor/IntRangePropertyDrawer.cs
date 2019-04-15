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
					Rect labelPosition = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
					EditorGUI.LabelField(labelPosition, label);

					Rect minLabelPosition = new Rect(labelPosition.x + labelPosition.width, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
					int[] values = new int[] { range._min, range._max };
					EditorGUI.MultiIntField(minLabelPosition, new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);

					return new IntRange(range._min, range._max);
				}
			}
		}
	}
}