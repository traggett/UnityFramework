using UnityEditor;
using UnityEngine;

namespace Engine
{
	using Maths;

	namespace JSON
	{
		[JSONEditor(typeof(IntRange), "PropertyField")]
		public static class JSONIntRangeEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				IntRange intRange = (IntRange)obj;
				EditorGUILayout.LabelField(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
				Rect rect = GUILayoutUtility.GetLastRect();
				EditorGUI.BeginChangeCheck();
				float[] values = new float[] { (float)intRange._min, (float)intRange._max };
				EditorGUI.MultiFloatField(rect, new GUIContent(label), new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);
				if (EditorGUI.EndChangeCheck())
				{
					intRange._min = Mathf.RoundToInt(values[0]);
					intRange._max = Mathf.RoundToInt(values[1]);
					dataChanged = true;
				}

				dataChanged = false;
				return intRange;
			}
			#endregion
		}
	}
}