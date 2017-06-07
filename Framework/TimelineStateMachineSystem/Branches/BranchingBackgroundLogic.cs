namespace Framework
{
	using StateMachineSystem;
	using System.Collections;

	namespace TimelineStateMachineSystem
	{
		public abstract class BranchingBackgroundLogic : IBranch
		{
			#region Virtual Interface
			public abstract void OnLogicStarted(StateMachine stateMachine);
			public abstract void OnLogicFinished(StateMachine stateMachine);
			public abstract void UpdateLogic(StateMachine stateMachine);

#if UNITY_EDITOR
			public abstract string GetDescription();
#endif
			#endregion

			#region IBranch
			public bool ShouldBranch(StateMachine stateMachine)
			{
				UpdateLogic(stateMachine);
				return false;
			}

			public IEnumerator GetGoToState(StateMachine stateMachine)
			{
				return null;
			}

			public void OnBranchingStarted(StateMachine stateMachine)
			{
				OnLogicStarted(stateMachine);
			}

			public void OnBranchingFinished(StateMachine stateMachine)
			{
				OnLogicFinished(stateMachine);
			}
			#endregion
		}
	}
}