using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Vector3), "PropertyField")]
		public static class Vector3Editor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Vector3Field(label, (Vector3)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}