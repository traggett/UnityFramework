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
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Vector3Field(label, (Vector3)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}