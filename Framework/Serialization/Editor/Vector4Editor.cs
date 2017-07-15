using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Vector4), "PropertyField")]
		public static class Vector4Editor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.Vector4Field(label, (Vector4)obj);
				if (EditorGUI.EndChangeCheck())
					dataChanged = true;

				return obj;
			}
			#endregion
		}
	}
}