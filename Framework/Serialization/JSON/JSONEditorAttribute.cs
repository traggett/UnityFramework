using System;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Engine
{
	namespace JSON
	{
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
		public sealed class JSONEditorAttribute : Attribute
		{
			#region Public Data
#if UNITY_EDITOR
			public delegate object RenderPropertiesDelegate(object obj, GUIContent label, out bool dataChanged);

			public readonly Type ObjectType;
			public readonly string OnRenderPropertiesMethod;
#endif
			#endregion

			#region Public Interface
			public JSONEditorAttribute(Type objectType, string onRenderPropertiesMethod)
			{
#if UNITY_EDITOR
				ObjectType = objectType;
				OnRenderPropertiesMethod = onRenderPropertiesMethod;
#endif
			}
			#endregion
		}
	}
}