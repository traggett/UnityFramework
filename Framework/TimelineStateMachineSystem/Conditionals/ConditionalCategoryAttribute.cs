using System;

namespace Framework
{
	namespace TimelineStateMachineSystem
	{
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		public sealed class ConditionalCategoryAttribute : Attribute
		{
#if UNITY_EDITOR
			public readonly string Category;
#endif

			public ConditionalCategoryAttribute(string category)
			{
#if UNITY_EDITOR
				Category = category;
#endif
			}
		}
	}
}