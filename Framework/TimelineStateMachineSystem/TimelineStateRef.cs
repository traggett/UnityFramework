using System;

using UnityEngine;


namespace Framework
{
	using Utils;
	using Serialization;
	
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public struct TimelineStateRef
		{
			#region Public Data		
			public AssetRef<TextAsset> _file;
			public int _stateId;
			//Editor properties
			public Vector2 _editorExternalLinkPosition;
			#endregion

			#region Private Data
			private TimelineStateMachine _stateMachine;
			private TimelineState _timelineState;

#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorFoldout;
			[NonSerialized]
			public string _editorStateName;
#endif
			#endregion

			#region Public Interface
			public static implicit operator string(TimelineStateRef property)
			{
#if UNITY_EDITOR
				return property.GetStateName();
#else
				return property._file;
#endif
			}

			public void FixUpRef(TimelineStateMachine stateMachine)
			{
				_stateMachine = stateMachine;
				_timelineState = null;
#if UNITY_EDITOR
				_editorStateName = null;
#endif
			}

			public bool IsValid()
			{
				return _stateId != -1 || _file.IsValid();
			}

			public TimelineState GetTimelineState(GameObject sourceObject = null)
			{
				if (_timelineState == null)
				{
					//If file path is invalid then its an internal state.
					if (IsInternal())
					{
						if (_stateMachine != null)
						{
							foreach (TimelineState state in _stateMachine._states)
							{
								if (state._stateId == _stateId)
								{
									_timelineState = state;
									break;
								}
							}
						}
						else
						{
							throw new Exception("TimelineStateRefProperty need to be fixed up by TimelineStateMachine before allowing access to internal state.");
						}
					}
					//Otherwise its pointing at an external asset.
					else
					{
						TextAsset asset = _file.LoadAsset();
						_stateMachine = TimelineStateMachine.FromTextAsset(asset, sourceObject);
						_file.UnloadAsset();
						_timelineState = _stateMachine.GetTimelineState(_stateId);
					}
				}

				return _timelineState;
			}

			public bool IsInternal()
			{
				return _file == null || !_file.IsValid();
			}

#if UNITY_EDITOR
			public TimelineStateMachine GetStateMachine()
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
					if (_stateMachine != null && GetTimelineState() != null)
					{
						_editorStateName = StringUtils.GetFirstLine(_timelineState.GetDescription());
					}
				}
				else
				{
					if (_file._editorAsset != null)
					{
						TimelineStateMachine stateMachines = Serializer.FromTextAsset<TimelineStateMachine>(_file._editorAsset);
						TimelineState[] states = stateMachines._states;

						foreach (TimelineState state in states)
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