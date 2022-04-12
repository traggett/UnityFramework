using System;

namespace Framework
{
	namespace TimelineSystem
	{
		[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
		public sealed class EventCategoryAttribute : Attribute
		{
			public const string kCoreEvent = "#core#";
#if UNITY_EDITOR
			public readonly string Category;
#endif

			public EventCategoryAttribute(string category = kCoreEvent)
			{
#if UNITY_EDITOR
				Category = category;
#endif
			}
		}
	}
}