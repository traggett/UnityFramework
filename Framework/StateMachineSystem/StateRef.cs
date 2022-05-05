using System;
using UnityEngine;

namespace Framework
{
	using Utils;
	
	namespace StateMachineSystem
	{
		[Serializable]
		public struct StateRef
		{
			#region Serialized Data
			[SerializeField]
			private State _state;

			//Editor properties
			[HideInInspector]
			public Vector2 _editorPosition;
			#endregion

			#region Public Data
			public State State
			{
				get
				{
					return _state;
				}
				
			}
			#endregion

			#region Public Interface
#if UNITY_EDITOR
			public StateRef(State state)
			{
				_state = state;
				_editorPosition = new Vector2(0f, 0f);
			}
#endif
			#endregion
		}
	}
}