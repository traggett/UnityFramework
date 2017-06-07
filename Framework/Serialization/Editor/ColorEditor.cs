using UnityEngine;
using UnityEditor;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Color), "PropertyField")]
		public static class ColorEditor
		{
			#region SerializedObjectEditor
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