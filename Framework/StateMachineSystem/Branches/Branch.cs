using System;
using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public sealed class Branch : IBranch
		{
			#region Public Data		
			public Condition _condition;
			public StateRef _goToState;
			#endregion

			#region Public Interface
#if UNITY_EDITOR
			public string GetDescription()
			{
				string description = "If ";

				if (_condition._not)
					description += "<b>not</b> ";

				if (_condition._conditional != null)
					description += _condition._conditional.GetEditorDescription();
				else
					description += "<condition>";

				return description;
			}
#endif
			#endregion

			#region IBranch
			public bool ShouldBranch(StateMachineComponent stateMachine)
			{
				if (_condition._conditional != null)
				{
					return _condition._conditional.IsConditionMet(stateMachine) != _condition._not;
				}

				return false;
			}

			public IEnumerator GetGoToState(StateMachineComponent stateMachine)
			{
				return _goToState.PerformState(stateMachine);
			}

			public void OnBranchingStarted(StateMachineComponent stateMachine)
			{
				if (_condition._conditional != null)
					_condition._conditional.OnStartConditionChecking(stateMachine);
			}

			public void OnBranchingFinished(StateMachineComponent stateMachine)
			{
				if (_condition._conditional != null)
					_condition._conditional.OnEndConditionChecking(stateMachine);
			}
			#endregion
		}
	}
}