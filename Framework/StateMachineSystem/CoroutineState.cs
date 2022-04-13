using System;
using System.Collections;

namespace Framework
{
	using UnityEngine;
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class CoroutineState : State
		{
			#region Public Data		
			public CoroutineRef _coroutine;

			[StateLink("And then")]
			public StateRef _goToState;
			#endregion

			#region State
#if UNITY_EDITOR
			public override string GetEditorLabel()
			{
				return "Coroutine (State" + _stateId.ToString("00") + ")";
			}

			public override string GetEditorDescription()
			{
				return "Run " + _coroutine;
			}

			public override Color GetEditorColor()
			{
				return new Color(156 / 255f, 68 / 255f, 68 / 255f);
			}
#endif

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				yield return _coroutine.RunCoroutine();

				stateMachine.GoToState(StateMachine.Run(stateMachine, _goToState));
			}
			#endregion
		}
	}
}