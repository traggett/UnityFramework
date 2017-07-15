using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Gradient), "PropertyField")]
		public static class GradientEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				EditorGUILayout.LabelField(GUIContent.none, GUILayout.Height(EditorGUIUtility.singleLineHeight), GUILayout.ExpandWidth(true));
				Rect rect = GUILayoutUtility.GetLastRect();
				EditorGUI.BeginChangeCheck();
				Gradient gradient = EditorUtils.GradientField(label, rect, (Gradient)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return gradient;
			}
			#endregion
		}
	}
}