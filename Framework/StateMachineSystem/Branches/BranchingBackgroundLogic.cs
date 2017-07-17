namespace Framework
{
	namespace StateMachineSystem
	{
		public abstract class BranchingBackgroundLogic
		{
			#region Virtual Interface
			public abstract void OnLogicStarted(StateMachineComponent stateMachine);
			public abstract void OnLogicFinished(StateMachineComponent stateMachine);
			public abstract void UpdateLogic(StateMachineComponent stateMachine);

#if UNITY_EDITOR
			public abstract string GetDescription();
#endif
			#endregion
		}
	}
}