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
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Vector2Field(label, (Vector2)obj);
				dataChanged = EditorGUI.EndChangeCheck();
				return obj;
			}
			#endregion
		}
	}
}