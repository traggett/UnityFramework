using UnityEngine;

using System;
using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public abstract class State
		{
			#region Public Data
			[HideInInspector]	
			public int _stateId = -1;

			//Editor properties
			[HideInInspector]
			public Vector2 _editorPosition = Vector2.zero;
			[HideInInspector]
			public bool _editorAutoDescription = true;
			[HideInInspector]
			public string _editorDescription;
			[HideInInspector]
			public bool _editorAutoColor = true;
			[HideInInspector]
			public Color _editorColor = Color.gray;
#if DEBUG
			[NonSerialized]
			public StateMachine _debugParentStateMachine;
#endif
			#endregion

			#region Public Interface
			public abstract IEnumerator PerformState(StateMachineComponent stateMachine);

#if UNITY_EDITOR
			public virtual string GetDescription() { return _editorDescription; }
			public virtual StateMachineEditorLink[] GetEditorLinks() { return null; }
#endif
			#endregion
		}
	}
}