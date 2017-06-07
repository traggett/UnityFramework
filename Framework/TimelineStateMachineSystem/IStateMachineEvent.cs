namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		public enum eEventTriggerReturn
		{
			EventOngoing,
			EventFinished,
			EventFinishedExitState,
		};

		public interface IStateMachineEvent
		{
			eEventTriggerReturn Trigger(StateMachine stateMachine);
			eEventTriggerReturn Update(StateMachine stateMachine, float eventTime);
			void End(StateMachine stateMachine);

#if UNITY_EDITOR
			EditorStateLink[] GetEditorLinks();
#endif
		}
	}
}