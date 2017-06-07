using System;

namespace Engine
{
	namespace JSON
	{
		[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
		public sealed class JSONFieldAttribute : Attribute
		{
			public readonly string ID;
			public readonly bool HideInEditor;

			public JSONFieldAttribute(string id = null, bool hideInEditor = false)
			{
				ID = id;
				HideInEditor = hideInEditor;
			}
		}
	}
}