namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		public interface IConditional
		{
			#region Virtual Interface
			void OnStartConditionChecking(StateMachineComponent stateMachine);
			bool IsConditionMet(StateMachineComponent stateMachine);
			void OnEndConditionChecking(StateMachineComponent stateMachine);

#if UNITY_EDITOR
			string GetEditorDescription();
			bool AllowInverseVariant();
#endif
			#endregion
		}
	}
}