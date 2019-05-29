using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(int), "PropertyField")]
		public static class IntEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.IntField(label, (int)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}