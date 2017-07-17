using System;
using System.Collections;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Framework
{
	using StateMachineSystem;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public struct StateRefProperty
		{
			[SerializeField]
			private TextAsset _file;
			[SerializeField]
			private int _timelineId;

			public StateRefProperty(TextAsset file=null, int TimelineId=-1)
			{
				_file = file;
				_timelineId = TimelineId;
			}

			public State LoadTimelineState(GameObject sourceObject = null)
			{
				if (_file != null)
				{
					StateMachine stateMachine = StateMachine.FromTextAsset(_file, sourceObject);
					return stateMachine.GetState(_timelineId);
				}

				return null;
			}

			public bool IsValid()
			{
				return _file != null;
			}
			
			public TextAsset GetFile()
			{
				return _file;
			}

			public IEnumerator PerformState(StateMachineComponent stateMachine, GameObject sourceObject = null)
			{
				State state = LoadTimelineState(sourceObject != null ? sourceObject : stateMachine.gameObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = AssetDatabase.GetAssetPath(_file);
					StateMachineDebug.OnStateStarted(stateMachine, state, debugFileName);
#endif
					return state.PerformState(stateMachine);
				}

				return null;
			}

		}
	}
}