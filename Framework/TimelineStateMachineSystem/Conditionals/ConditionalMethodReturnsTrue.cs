using UnityEngine;
using System;
using System.Reflection;

namespace Framework
{
	using StateMachineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("")]
		public class ConditionalMethodReturnsTrue : IConditional
		{
			public ComponentMethodRef<bool> _method;
			
			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				return "(<b>" + _method + "</b>)";
			}

			public bool AllowInverseVariant()
			{
				return true;
			}
#endif

			public void OnStartConditionChecking(StateMachine stateMachine)
			{

			}
			
			public bool IsConditionMet(StateMachine stateMachine)
			{
				if (_method.RunMethod())
				{
					return true;
				}

				return false;
			}

			public void OnEndConditionChecking(StateMachine stateMachine)
			{

			}
			#endregion
		}
	}
}