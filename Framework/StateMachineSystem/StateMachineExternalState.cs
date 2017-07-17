#if UNITY_EDITOR
using System;
using System.Collections;
using Framework.StateMachineSystem;

namespace Framework
{
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public class StateMachineExternalState : State
		{
			public StateRef _externalStateRef;

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}

			public override string GetDescription()
			{
				return (_externalStateRef._file._editorAsset != null ? _externalStateRef._file._editorAsset.name : null);
			}
		}
	}
}

#endif