using System;
using System.Collections;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class CoroutineState : State
		{
			#region Public Data		
			public CoroutineRef _coroutine;
			public StateRef _goToState;
			#endregion

			#region State
			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				yield return _coroutine.RunCoroutine();

				stateMachine.GoToState(StateMachine.Run(stateMachine, _goToState));
			}
			
#if UNITY_EDITOR
			public override StateMachineEditorLink[] GetEditorLinks()
			{
				StateMachineEditorLink[] links = new StateMachineEditorLink[1];
				links[0] = new StateMachineEditorLink(this, "goToState", "Coroutine Done");
				return links;
			}

			public override string GetAutoDescription()
			{
				string label = "Run " + _coroutine;
				return label;
			}

			public override string GetStateIdLabel()
			{
				return "Coroutine (State" + _stateId.ToString("00") + ")";
			}
#endif
			#endregion
		}
	}
}