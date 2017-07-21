using UnityEngine;
using UnityEditor;


namespace Framework
{
	using Utils.Editor;

	namespace Serialization
	{
		[SerializedObjectEditor(typeof(string), "PropertyField")]
		public static class StringEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField(label, GUILayout.Width(EditorUtils.GetLabelWidth()));
				obj = EditorGUILayout.TextArea((string)obj);
				EditorGUILayout.EndHorizontal();
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}