using System;
using System.Collections;

using UnityEngine;

namespace Framework
{
	using Utils;
	using Serialization;
	
	namespace StateMachineSystem
	{
		[Serializable]
		public struct StateRef
		{
			#region Public Data		
			public AssetRef<TextAsset> _file;
			public int _stateId;
			//Editor properties
			public Vector2 _editorExternalLinkPosition;
			#endregion

			#region Private Data
			private StateMachine _stateMachine;
			private State _state;
#if UNITY_EDITOR
			//Editor properties
			[NonSerialized]
			public bool _editorCollapsed;
			[NonSerialized]
			public string _editorStateName;
#endif
			#endregion

			#region Public Interface
			public static implicit operator string(StateRef property)
			{
#if UNITY_EDITOR
				return StringUtils.RemoveRichText(property.GetStateName());
#else
				return property._file;
#endif
			}

			public void FixUpRef(StateMachine stateMachine)
			{
				_stateMachine = stateMachine;
				_state = null;
#if UNITY_EDITOR
				_editorStateName = null;
#endif
			}

			public bool IsValid()
			{
				return _stateId != -1 || _file.IsValid();
			}

			public State GetState(GameObject sourceObject = null)
			{
				if (_state == null)
				{
					//If file path is invalid then its an internal state.
					if (IsInternal())
					{
						if (_stateMachine != null)
						{
							_state = _stateMachine.GetState(_stateId);
						}
						else
						{
							throw new Exception("StateRefProperty need to be fixed up by TimelineStateMachine before allowing access to internal state.");
						}
					}
					//Otherwise its pointing at an external asset.
					else
					{
						TextAsset asset = _file.LoadAsset();
						_stateMachine = StateMachine.FromTextAsset(asset, sourceObject);
						_file.UnloadAsset();
						_state = _stateMachine.GetState(_stateId);
					}
				}

				return _state;
			}

			public bool IsInternal()
			{
				return _file == null || !_file.IsValid();
			}

			public IEnumerator PerformState(StateMachineComponent stateMachine, GameObject sourceObject = null)
			{
				State state = GetState(sourceObject != null ? sourceObject : stateMachine.gameObject);

				if (state != null)
				{
#if UNITY_EDITOR && DEBUG
					string debugFileName = _file.GetFilePath();
					StateMachineDebug.OnStateStarted(stateMachine, state, debugFileName);
#endif
					return state.PerformState(stateMachine);
				}

				return null;
			}

#if UNITY_EDITOR
			public StateMachine GetStateMachine()
			{
				return _stateMachine;
			}

			public int GetStateID()
			{
				return _stateId;
			}

			public string GetStateName()
			{
				if (_editorStateName == null)
				{
					UpdateStateName();
				}

				return _editorStateName;
			}
#endif
			#endregion
			
			#region Private Functions
#if UNITY_EDITOR
			private void UpdateStateName()
			{
				_editorStateName = "<none>";
						
				if (IsInternal())
				{
					if (_stateMachine != null && GetState() != null)
					{
						_editorStateName = StringUtils.GetFirstLine(_state.GetDescription());
					}
				}
				else
				{
					if (_file._editorAsset != null)
					{
						StateMachine stateMachines = Serializer.FromTextAsset<StateMachine>(_file._editorAsset);
						State[] states = stateMachines._states;

						foreach (State state in states)
						{
							if (state._stateId == _stateId)
							{
								_editorStateName = _file._editorAsset.name + ":" + StringUtils.GetFirstLine(state.GetDescription());
								break;
							}
						}
					}
				}
			}
#endif
			#endregion
		}
	}
}