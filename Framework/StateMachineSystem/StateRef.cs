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
			#region Serialized Data		
			[SerializeField]
			private AssetRef<TextAsset> _file;
			[SerializeField]
			private int _stateId;
			#endregion

			#region Private Data
			private StateMachine _parentStateMachine;
			private State _state;
#if UNITY_EDITOR
			//Editor properties
			public Vector2 _editorExternalLinkPosition;
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

			public void SetParentStatemachine(StateMachine stateMachine)
			{
				_parentStateMachine = stateMachine;
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
						if (_parentStateMachine != null)
						{
							_state = _parentStateMachine.GetState(_stateId);
						}
						else
						{
							throw new Exception("StateRefProperty need to be fixed up by a StateMachine before allowing access to internal state.");
						}
					}
					//Otherwise its pointing at an external asset.
					else
					{
						TextAsset asset = _file.LoadAsset();
						_parentStateMachine = StateMachine.FromTextAsset(asset, sourceObject);
						_file.UnloadAsset();
						_state = _parentStateMachine.GetState(_stateId);
					}
				}

				return _state;
			}

			public bool IsInternal()
			{
				return _stateId != -1 && !_file.IsValid();
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
			public StateRef(int stateId, StateMachine parentStateMachine = null)
			{
				_file = new AssetRef<TextAsset>();
				_stateId = stateId;
				_editorExternalLinkPosition = Vector2.zero;
				_parentStateMachine = parentStateMachine;
				_state = null;
				_editorCollapsed = false;
				_editorStateName = null;
			}

			public StateRef(TextAsset asset, int stateId, StateMachine parentStateMachine = null)
			{
				_file = new AssetRef<TextAsset>(asset);
				_stateId = stateId;
				_editorExternalLinkPosition = Vector2.zero;
				_parentStateMachine = parentStateMachine;
				_state = null;
				_editorCollapsed = false;
				_editorStateName = null;
			}

			public StateMachine GetParentStateMachine()
			{
				return _parentStateMachine;
			}

			public string GetStateName()
			{
				if (_editorStateName == null)
				{
					UpdateStateName();
				}

				return _editorStateName;
			}

			public string GetStateMachineName()
			{
				if (IsInternal())
				{
					if (_parentStateMachine != null)
						return _parentStateMachine._name;
					else
						return "StateMachine";
				}
				else
				{
					return _file.GetAssetName();
				}
			}

			public int GetStateID()
			{
				return _stateId;
			}

			public AssetRef<TextAsset> GetExternalFile()
			{
				return _file;
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
					if (_parentStateMachine != null && GetState() != null)
					{
						_editorStateName = GetDescription(_state);
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
								_editorStateName = _file._editorAsset.name + ":" + GetDescription(_state);
								break;
							}
						}
					}
				}
			}

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