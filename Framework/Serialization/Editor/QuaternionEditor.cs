using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace Serialization
	{
		[SerializedObjectEditor(typeof(Quaternion), "PropertyField")]
		public static class QuaternionEditor
		{
			#region SerializedObjectEditor
			public static object PropertyField(object obj, GUIContent label, ref bool dataChanged)
			{
				Quaternion quaternion = (Quaternion)obj;
				EditorGUI.BeginChangeCheck();
				Vector3 euler = EditorGUILayout.Vector3Field(label, quaternion.eulerAngles);
				if (EditorGUI.EndChangeCheck())
				{
					dataChanged = true;
					return Quaternion.Euler(euler);
				}

				return obj;
			}
			#endregion
		}
	}
}