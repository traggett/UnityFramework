using System;

using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Enum), "PropertyField")]
		public static class EnumEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.EnumPopup(label, (Enum)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}