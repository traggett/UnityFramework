using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		namespace Editor
		{
			[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
			public sealed class StateCustomEditorGUIAttribute : Attribute
			{
				public readonly Type StateType;

				public StateCustomEditorGUIAttribute(Type stateType)
				{
					StateType = stateType;
				}
			}
		}
	}
}