using UnityEngine;
using UnityEditor;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(string), "PropertyField")]
		public static class JSONStringEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(label, GUILayout.Width(EditorGUIUtility.labelWidth - (EditorGUI.indentLevel * 15.0f) - 4.0f));
				obj = EditorGUILayout.TextArea((string)obj);
				EditorGUILayout.EndHorizontal();
				dataChanged = EditorGUI.EndChangeCheck();

				return obj;
			}
			#endregion
		}
	}
}