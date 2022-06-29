using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		public class StateMachineComponent : CoroutineStateMachine
		{
			#region Public Interface
			public void RunStateMachine(StateMachine stateMachine)
			{
				GoToState(stateMachine._entryState._initialState);
			}

			public void GoToState(StateRef stateRef)
			{
				GoToState(stateRef.State);
			}

			public void GoToState(State state)
			{
				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					StateMachineDebug.OnEnterState(this, state);
#endif
					OnEnterState(state);
					GoToState(state.PerformState(this));
				}
				else
				{
					Stop();
				}
			}
			#endregion

			#region Virtual Interface
			protected virtual void OnEnterState(State state)
			{

			}
			#endregion
		}
	}
}