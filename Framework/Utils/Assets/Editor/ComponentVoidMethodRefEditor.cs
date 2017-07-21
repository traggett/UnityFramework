using UnityEngine;

namespace Framework
{
	using Serialization;

	namespace Utils
	{
		namespace Editor
		{
			[SerializedObjectEditor(typeof(ComponentVoidMethodRef), "PropertyField")]
			public static class ComponentVoidMethodRefEditor
			{
				#region SerializedObjectEditor
				public static object PropertyField(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options)
				{
					ComponentVoidMethodRef componentMethodRef = (ComponentVoidMethodRef)obj;
					return ComponentMethodRefEditor.ComponentMethodRefField(componentMethodRef.GetMethodRef(), typeof(void), label, ref dataChanged);
				}
				#endregion
			}
		}
	}
}