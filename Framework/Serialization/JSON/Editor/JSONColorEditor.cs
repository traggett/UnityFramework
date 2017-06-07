using UnityEngine;
using UnityEditor;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(Color), "PropertyField")]
		public static class JSONColorEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.ColorField(label, (Color)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}