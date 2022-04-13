using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
		public class StateLinkAttribute : Attribute
		{
			public readonly string _editorName;

			public StateLinkAttribute(string editorName)
			{
				_editorName = editorName;
			}
		}
	}
}