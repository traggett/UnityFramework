using System;
using UnityEngine;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public class StateMachine : ScriptableObject
		{
			#region Public Data
			[HideInInspector]
			public StateMachineEntryState _entryState;
			#endregion
		}
	}
}