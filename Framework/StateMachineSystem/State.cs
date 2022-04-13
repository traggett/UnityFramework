using UnityEngine;

using System;
using System.Collections;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public abstract class State : ScriptableObject
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
			public virtual string GetEditorLabel()
			{
				return "State" + _stateId.ToString("00") + " (" + GetType().Name + ")";
			}

			public virtual string GetEditorDescription() 
			{
				return GetType().Name;
			}

			public virtual Color GetEditorColor()
			{
				return new Color(61f / 255f, 154f / 255f, 92f / 255f);
			}
#endif
			#endregion
		}
	}
}