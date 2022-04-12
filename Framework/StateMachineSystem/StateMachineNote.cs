#if UNITY_EDITOR

using System;
using System.Collections;

namespace Framework
{
	using Utils;

	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachineNote : State
		{
			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}

			public override string GetStateIdLabel()
			{
				return "Note";
			}
		}
	}
}

#endif