using UnityEditor;
using UnityEngine;

namespace Engine
{
	using Maths;

	namespace JSON
	{
		[JSONEditor(typeof(FloatRange), "PropertyField")]
		public static class JSONFloatRangeEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
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

				dataChanged = false;
				return floatRange;
			}
			#endregion
		}
	}
}