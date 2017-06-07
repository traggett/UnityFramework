namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		public interface IConditional
		{
			#region Virtual Interface
			void OnStartConditionChecking(StateMachine stateMachine);
			bool IsConditionMet(StateMachine stateMachine);
			void OnEndConditionChecking(StateMachine stateMachine);

#if UNITY_EDITOR
			string GetEditorDescription();
			bool AllowInverseVariant();
#endif
			#endregion
		}
	}
}