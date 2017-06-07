using UnityEditor;
using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(float), "PropertyField")]
		public static class JSONFloatEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.FloatField(label, (float)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}