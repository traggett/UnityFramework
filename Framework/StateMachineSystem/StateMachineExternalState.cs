#if UNITY_EDITOR

using System;
using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachineExternalState : State
		{
			[NonSerialized]
			public StateMachineEditorLink _externalStateRef;

			public override IEnumerator PerformState(StateMachineComponent stateMachine)
			{
				throw new NotImplementedException();
			}

			public override string GetDescription()
			{
				StateRef stateRef = _externalStateRef.GetStateRef();
				return (stateRef._file._editorAsset != null ? stateRef._file._editorAsset.name : null);
			}
		}
	}
}

#endif