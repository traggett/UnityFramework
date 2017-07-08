using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace Serialization
	{
		[SerializedObjectEditor(typeof(IntRange), "PropertyField")]
		public static class IntRangeEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				dataChanged = false;

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

				
				return intRange;
			}
			#endregion
		}
	}
}