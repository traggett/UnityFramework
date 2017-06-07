using System;

using UnityEditor;
using UnityEngine;

namespace Engine
{
	namespace JSON
	{
		[JSONEditor(typeof(Enum), "PropertyField")]
		public static class JSONEnumEditor
		{
			#region JSONObjectEditor
			public static object PropertyField(object obj, GUIContent label, out bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.EnumPopup(label, (Enum)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				
				return obj;
			}
			#endregion
		}
	}
}