using System;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachine : ScriptableObject, ISerializationCallbackReceiver
		{
			#region Public Data
			[HideInInspector]
			public StateMachineEntryState _entryState;
			[HideInInspector]
			public State[] _states = new State[0];
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if DEBUG
				foreach (State state in _states)
				{
					if (state != null)
						state._debugParentStateMachine = this;
				}			
#endif
			}
			#endregion
		}
	}
}