using System;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		[ConditionCategory("")]
		public class ConditionStateMachineIsRunning : ToggableCondition
		{
			public ComponentRef<StateMachineComponent> _stateMachine;

			private StateMachineComponent _stateMachineObj;

			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				return "If (" + _stateMachine + ") is running";
			}

			public override string GetTakenText()
			{
				return _stateMachine + " stopped.";
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				_stateMachineObj = _stateMachine.GetComponent();
			}
			
			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				if (_stateMachineObj != null && _stateMachineObj.IsRunning())
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