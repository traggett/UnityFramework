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
			public ComponentRef<StateMachine> _stateMachine;

			private StateMachine _stateMachineObj;

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

			public void OnStartConditionChecking(StateMachine stateMachine)
			{
				_stateMachineObj = _stateMachine.GetComponent();
			}
			
			public bool IsConditionMet(StateMachine stateMachine)
			{
				if (_stateMachineObj != null && _stateMachineObj.IsRunning())
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