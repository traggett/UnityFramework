using System;
using System.Collections;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public sealed class ConditionalStateBranch
		{
			#region Public Data		
			public Condition _condition;
			public StateRef _goToState;
			#endregion

			#region Public Interface
#if UNITY_EDITOR
			public string GetDescription()
			{
				string description = "";

				if (_condition is ToggableCondition && ((ToggableCondition)_condition)._not)
					description += "<b>!</b> ";

				if (_condition != null)
					description += _condition.GetDescription();
				else
					description += "<condition>";

				return description;
			}

			public string GetTakenText()
			{
				if (_condition != null)
					return _condition.GetTakenText();
				else
					return "Branch Taken";
			}	
#endif

			public bool ShouldBranch(StateMachineComponent stateMachine)
			{
				if (_condition != null)
				{
					bool conditionMet = _condition.IsConditionMet(stateMachine);

					if (_condition is ToggableCondition && ((ToggableCondition)_condition)._not)
						conditionMet = !conditionMet;

					return conditionMet;
				}

				return false;
			}

			public IEnumerator GetGoToState(StateMachineComponent stateMachine)
			{
				return _goToState.PerformState(stateMachine);
			}

			public void OnBranchingStarted(StateMachineComponent stateMachine)
			{
				if (_condition != null)
					_condition.OnStartConditionChecking(stateMachine);
			}

			public void OnBranchingFinished(StateMachineComponent stateMachine)
			{
				if (_condition != null)
					_condition.OnEndConditionChecking(stateMachine);
			}
			#endregion
		}
	}
}