using UnityEditor;
using UnityEngine;

namespace Engine
{
	using Utils.Editor;

	namespace JSON
	{
		[JSONEditor(typeof(Gradient), "PropertyField")]
		public static class JSONGradientEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUILayout.LabelField(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
				Rect rect = GUILayoutUtility.GetLastRect();
				EditorGUI.BeginChangeCheck();
				Gradient gradient = EditorUtils.GradientField(label, rect, (Gradient)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return gradient;
			}
			#endregion
		}
	}
}