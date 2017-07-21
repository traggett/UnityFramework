using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Vector2), "PropertyField")]
		public static class Vector2Editor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Vector2Field(label, (Vector2)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}