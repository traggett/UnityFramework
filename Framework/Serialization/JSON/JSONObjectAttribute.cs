 using System;

namespace Engine
{
	namespace JSON
	{
		[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
		public sealed class JSONObjectAttribute : Attribute
		{
			#region Public Data
			public readonly string JSONTag;
			#endregion

			#region Public Interface
			public JSONObjectAttribute(string JSONTag = null)
			{
				JSONTag = JSONTag;
			}
			#endregion
		}
	}
}