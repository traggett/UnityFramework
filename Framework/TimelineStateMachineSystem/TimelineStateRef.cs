using System;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils;
	using Serialization;
	
	namespace TimelineStateMachineSystem
	{
		[Serializable]
		public sealed class TimelineStateRef : ISerializationCallbackReceiver, ICustomEditorInspector
		{
			#region Public Data		
			public AssetRef<TextAsset> _file;
			public int _stateId = -1;
			//Editor properties
			public Vector2 _editorExternalLinkPosition = Vector2.zero;
			#endregion

			#region Private Data
			private TimelineStateMachine _stateMachine;
			private TimelineState _timelineState;

#if UNITY_EDITOR
			private bool _editorFoldout = true;
			private enum eType
			{
				Internal,
				External
			}
			private eType _editorLinkType;
			private string _editorStateName;
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
					if (string.IsNullOrEmpty(_file._filePath))
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
							throw new Exception("TimelineStateRefProperty need to be fixed up by TimelineStateMachine");
						}
					}
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

#if UNITY_EDITOR
			public bool IsInternal()
			{
				return _editorLinkType == eType.Internal;
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

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if UNITY_EDITOR
				_editorLinkType = _file != null && _file._editorAsset != null ? eType.External : eType.Internal;
#endif
			}
			#endregion

			#region ICustomEditable
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				if (label == null)
					label = new GUIContent();

				label.text += " (" + this + ")";

				_editorFoldout = EditorGUILayout.Foldout(_editorFoldout, label);

				if (_editorFoldout)
				{
					int origIndent = EditorGUI.indentLevel;
					EditorGUI.indentLevel++;

					eType type = (eType)EditorGUILayout.EnumPopup("Link Type", _editorLinkType);

					//If link type changed, clear info
					if (_editorLinkType != type)
					{
						_editorLinkType = type;
						_file.ClearAsset();
						_stateId = -1;
						_timelineState = null;
						_editorStateName = null;
						dataChanged = true;
					}
					
					switch (_editorLinkType)
					{
						case eType.Internal:
							{
								dataChanged |= DrawStateNamePopUps();
							}
							break;
						case eType.External:
							{
								TextAsset asset = EditorGUILayout.ObjectField("File", _file._editorAsset, typeof(TextAsset), false) as TextAsset;

								//If asset changed update GUIDS
								if (_file._editorAsset != asset)
								{
									_file._editorAsset = asset;

									_file.ClearAsset();
									_stateId = -1;
									_timelineState = null;
									_editorStateName = null;

									if (asset != null)
									{
										_file.SetAsset(asset);
									}

									dataChanged = true;
								}

								if  (_file._editorAsset != null)
								{
									dataChanged |= DrawStateNamePopUps();
								}
							}
							break;
					}
					
					EditorGUI.indentLevel = origIndent;
				}

				return dataChanged;
			}
#endif
			#endregion

			#region Private Functions
#if UNITY_EDITOR
			private void UpdateStateName()
			{
				_editorStateName = "<none>";

				switch (_editorLinkType)
				{
					case eType.External:
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
						break;
					case eType.Internal:
						{
							if (GetTimelineState() != null)
							{
								_editorStateName = StringUtils.GetFirstLine(_timelineState.GetDescription());
							}
						}
						break;
				}
			}

			private bool DrawStateNamePopUps()
			{
				TimelineState[] states = null;

				switch (_editorLinkType)
				{
					case eType.External:
						{
							TimelineStateMachine stateMachines = Serializer.FromFile<TimelineStateMachine>(AssetDatabase.GetAssetPath(_file._editorAsset));
							states = stateMachines._states;
						}
						break;
					case eType.Internal:
						{
							if (_stateMachine != null)
								states = _stateMachine._states;
						}
						break;
				}

				if (states != null && states.Length > 0)
				{
					string[] stateNames = new string[states.Length+1];
					int index = 0;
					stateNames[0] = "<none>";

					for (int i = 0; i < states.Length; i++)
					{
						stateNames[i+1] = "State"+ states[i]._stateId + " (" + StringUtils.GetFirstLine(states[i].GetDescription()) + ")";

						if (states[i]._stateId == _stateId)
						{
							index = i+1;
						}
					}

					EditorGUI.BeginChangeCheck();

					index = EditorGUILayout.Popup("Timeline", index, stateNames);

					if (EditorGUI.EndChangeCheck())
					{
						_stateId = index == 0 ? -1 : (int)states[index - 1]._stateId;
						_timelineState = null;
						_editorStateName = null;
						return true;
					}
				}

				return false;
			}
#endif
			#endregion
		}
	}
}