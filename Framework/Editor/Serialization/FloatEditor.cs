using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(float), "PropertyField")]
		public static class FloatEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.FloatField(label, (float)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}