using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		public sealed class ConditionCategoryAttribute : Attribute
		{
#if UNITY_EDITOR
			public readonly string Category;
#endif

			public ConditionCategoryAttribute(string category)
			{
#if UNITY_EDITOR
				Category = category;
#endif
			}
		}
	}
}