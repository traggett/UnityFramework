using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(string), "PropertyField")]
		public static class StringEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
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