using UnityEditor;
using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(bool), "PropertyField")]
		public static class JSONBoolEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Toggle(label, (bool)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}