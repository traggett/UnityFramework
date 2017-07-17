
using System;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
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

			public void OnStartConditionChecking(StateMachineComponent stateMachine)
			{

			}
			
			public bool IsConditionMet(StateMachineComponent stateMachine)
			{
				if (_method.RunMethod())
				{
					return true;
				}

				return false;
			}

			public void OnEndConditionChecking(StateMachineComponent stateMachine)
			{

			}
			#endregion
		}
	}
}