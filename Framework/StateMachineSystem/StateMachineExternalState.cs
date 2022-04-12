#if UNITY_EDITOR

using System;
using System.Collections;

namespace Framework
{
	using Utils;

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

			public override string GetAutoDescription()
			{
				StateRef stateRef = _externalStateRef.GetStateRef();
				return (stateRef.GetExternalFile()._editorAsset != null ? stateRef.GetExternalFile()._editorAsset.name : null);
			}

			public override string GetStateIdLabel()
			{
				return "External State";
			}
		}
	}
}

#endif