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
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				IntRange intRange = (IntRange)obj;
				EditorGUILayout.LabelField(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
				Rect rect = GUILayoutUtility.GetLastRect();
				EditorGUI.BeginChangeCheck();
				float[] values = new float[] { (float)intRange.Min, (float)intRange.Max };
				EditorGUI.MultiFloatField(rect, new GUIContent(label), new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);
				if (EditorGUI.EndChangeCheck())
				{
					intRange.Min = Mathf.RoundToInt(values[0]);
					intRange.Max = Mathf.RoundToInt(values[1]);
					dataChanged = true;
				}

				
				return intRange;
			}
			#endregion
		}
	}
}