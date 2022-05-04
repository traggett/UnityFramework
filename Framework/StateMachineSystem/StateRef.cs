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

			#region Public Interface
			public State GetState()
			{
				return _state;
			}

#if UNITY_EDITOR
			public StateRef(State state)
			{
				_state = state;
				_editorPosition = new Vector2(0f, 0f);
			}
#endif
			#endregion

			#region Private Functions
#if UNITY_EDITOR
			public string GetDescription(State state)
			{
				if (state._editorAutoDescription)
					return StringUtils.GetFirstLine(state.GetEditorDescription());

				return StringUtils.GetFirstLine(state._editorDescription);
			}
#endif
			#endregion
		}
	}
}