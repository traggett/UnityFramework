using System;

namespace Framework
{
	using StateMachineSystem;
	using Utils;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionalCategory("")]
		public class ConditionalStateMachineIsRunning : IConditional
		{
			public ComponentRef<StateMachineComponent> _stateMachine;

			private StateMachineComponent _stateMachineObj;

			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				return "(<b>" + _stateMachine + "</b>) is running";
			}

			public bool AllowInverseVariant()
			{
				return true;
			}
#endif

			public void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
				_stateMachineObj = _stateMachine.GetComponent();
			}
			
			public bool IsConditionMet(StateMachineComponent stateMachine)
			{
				if (_stateMachineObj != null && _stateMachineObj.IsRunning())
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