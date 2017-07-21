using System;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Framework
{
	namespace Serialization
	{
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
		public sealed class SerializedObjectEditorAttribute : Attribute
		{
			#region Public Data
#if UNITY_EDITOR
			public delegate object RenderPropertiesDelegate(object obj, GUIContent label, ref bool dataChanged, GUIStyle style, params GUILayoutOption[] options);

			public readonly Type ObjectType;
			public readonly string OnRenderPropertiesMethod;
			public readonly bool UseForChildTypes;
#endif
			#endregion

			#region Public Interface
			public SerializedObjectEditorAttribute(Type objectType, string onRenderPropertiesMethod, bool useForChildTypes = false )
			{
#if UNITY_EDITOR
				ObjectType = objectType;
				OnRenderPropertiesMethod = onRenderPropertiesMethod;
				UseForChildTypes = useForChildTypes;
#endif
			}
			#endregion
		}
	}
}