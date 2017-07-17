#if UNITY_EDITOR
using System;
using System.Collections;
using Framework.StateMachineSystem;

namespace Framework
{
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public class StateMachineNote : State
		{
			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}
		}
	}
}

#endif