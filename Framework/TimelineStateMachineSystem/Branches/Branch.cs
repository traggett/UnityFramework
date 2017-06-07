using System;
using System.Collections;

namespace Framework
{
	using StateMachineSystem;
	
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public sealed class Branch : IBranch
		{
			#region Public Data		
			public Condition _condition;
			public TimelineStateRef _goToState = new TimelineStateRef();
			#endregion

			#region Public Interface
#if UNITY_EDITOR
			public string GetDescription()
			{
				string description = "If ";

				if (_condition._not)
					description += "<b>not</b> ";

				if (_condition._condition != null)
					description += _condition._condition.GetEditorDescription();
				else
					description += "<condition>";

				return description;
			}
#endif
			#endregion

			#region IBranch
			public bool ShouldBranch(StateMachine stateMachine)
			{
				if (_condition._condition != null)
				{
					return _condition._condition.IsConditionMet(stateMachine) != _condition._not;
				}

				return false;
			}

			public IEnumerator GetGoToState(StateMachine stateMachine)
			{
				return TimelineStateMachine.Run(stateMachine, _goToState);
			}

			public void OnBranchingStarted(StateMachine stateMachine)
			{
				if (_condition._condition != null)
					_condition._condition.OnStartConditionChecking(stateMachine);
			}

			public void OnBranchingFinished(StateMachine stateMachine)
			{
				if (_condition._condition != null)
					_condition._condition.OnEndConditionChecking(stateMachine);
			}
			#endregion
		}
	}
}