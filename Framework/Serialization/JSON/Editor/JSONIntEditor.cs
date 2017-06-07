using UnityEditor;
using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(int), "PropertyField")]
		public static class JSONIntEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.IntField(label, (int)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}