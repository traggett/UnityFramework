using System;

namespace Framework
{
	namespace Debug
	{
		// Can be added to static parameterless functions, values and properties
		[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
		public sealed class DebugMenuAttribute : Attribute
		{
			public readonly string Path;

			public DebugMenuAttribute(string path = null)
			{
				Path = path;
			}
		}
	}
}