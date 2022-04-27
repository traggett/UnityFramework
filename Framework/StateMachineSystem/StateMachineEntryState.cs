using System;
using System.Collections;

namespace Framework
{
	using UnityEngine;
	
	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachineEntryState : State
		{
			[StateLink("Start")]
			public StateRef _initialState;

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}

			public override string GetEditorLabel()
			{
				return string.Empty;
			}

			public override string GetEditorDescription()
			{
				return string.Empty;
			}

			public override Color GetEditorColor()
			{
				return new Color(82 / 255f, 122 / 255f, 80 / 255f);
			}
		}
	}
}