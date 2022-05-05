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
					GoToState(state.PerformState(this));

#if UNITY_EDITOR && DEBUG
					if (IsRunning())
					{
						StateMachineDebug.OnEnterState(this, state);
					}
#endif
				}
				else
				{
					Stop();
				}
			}
			#endregion

			#region CoroutineStateMachine
			protected override void OnEnterState(IEnumerator state)
			{
#if UNITY_EDITOR && DEBUG
				StateMachineDebug.Clear(this);
#endif
			}

			protected override void OnStopped()
			{
#if UNITY_EDITOR && DEBUG
				StateMachineDebug.Clear(this);
#endif
			}
			#endregion
		}
	}
}