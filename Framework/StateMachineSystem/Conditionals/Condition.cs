using System;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public abstract class Condition
		{
			#region Private Data		
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorFoldout;

#endif
			#endregion

			#region Virtual Interface
			public abstract void OnStartConditionChecking(StateMachineComponent stateMachine);
			public abstract bool IsConditionMet(StateMachineComponent stateMachine);
			public abstract void OnEndConditionChecking(StateMachineComponent stateMachine);

#if UNITY_EDITOR
			public abstract string GetDescription();
			public abstract string GetTakenText();
#endif
			#endregion
		}
	}
}