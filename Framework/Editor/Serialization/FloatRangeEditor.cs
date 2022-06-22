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
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				FloatRange floatRange = (FloatRange)obj;
				EditorGUILayout.LabelField(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
				Rect rect = GUILayoutUtility.GetLastRect();
				EditorGUI.BeginChangeCheck();
				float[] values = new float[] { floatRange.Min, floatRange.Max };
				EditorGUI.MultiFloatField(rect, new GUIContent(label), new GUIContent[] { new GUIContent("f"), new GUIContent("t") }, values);
				if (EditorGUI.EndChangeCheck())
				{
					floatRange.Min = values[0];
					floatRange.Max = values[1];
					dataChanged = true;
				}

				return floatRange;
			}
			#endregion
		}
	}
}