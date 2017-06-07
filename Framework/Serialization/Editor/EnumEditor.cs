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