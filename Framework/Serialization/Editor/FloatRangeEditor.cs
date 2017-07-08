using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace Serialization
	{
		[SerializedObjectEditor(typeof(FloatRange), "PropertyField")]
		public static class FloatRangeEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				dataChanged = false;

				FloatRange floatRange = (FloatRange)obj;
				EditorGUILayout.LabelField(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
				Rect rect = GUILayoutUtility.GetLastRect();
				EditorGUI.BeginChangeCheck();
				float[] values = new float[] { floatRange._min, floatRange._max };
				EditorGUI.MultiFloatField(rect, new GUIContent(label), new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);
				if (EditorGUI.EndChangeCheck())
				{
					floatRange._min = values[0];
					floatRange._max = values[1];
					dataChanged = true;
				}

				return floatRange;
			}
			#endregion
		}
	}
}