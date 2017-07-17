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

		public interface ITimelineStateEvent
		{
			eEventTriggerReturn Trigger(StateMachineComponent stateMachine);
			eEventTriggerReturn Update(StateMachineComponent stateMachine, float eventTime);
			void End(StateMachineComponent stateMachine);

#if UNITY_EDITOR
			StateMachineEditorLink[] GetEditorLinks();
#endif
		}
	}
}