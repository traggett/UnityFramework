
using System;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		[ConditionCategory("")]
		public class ConditionMethodReturnsTrue : ToggableCondition
		{
			public ComponentMethodRef<bool> _method;
			
			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				return "If (" + _method + ")";
			}

			public override string GetTakenText()
			{
				return _method;
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{

			}
			
			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				if (_method.RunMethod())
				{
					return true;
				}

				return false;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{

			}
			#endregion
		}
	}
}