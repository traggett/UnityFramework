using System;

namespace Framework
{
	namespace TimelineSystem
	{
		namespace Editor
		{
			[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
			public sealed class EventCustomEditorGUIAttribute : Attribute
			{
				public readonly Type EventType;

				public EventCustomEditorGUIAttribute(Type eventType)
				{
					EventType = eventType;
				}
			}
		}
	}
}