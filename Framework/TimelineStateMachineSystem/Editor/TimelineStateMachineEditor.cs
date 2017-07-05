using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using TimelineSystem;
	using TimelineSystem.Editor;
	using StateMachineSystem;
	using LocalisationSystem;
	using Utils;
	using Utils.Editor;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		namespace Editor
		{
			public sealed class TimelineStateMachineEditor : SerializedObjectGridBasedEditor<TimelineState>, TimelineEditor.IEditor
			{
				#region Private Data
				private static readonly float kTopBorder = 53.0f;
				private static readonly float kDoubleClickTime = 0.32f;
				private static readonly float kArrowHeight = 6.0f;
				private static readonly float kArrowWidth = 4.0f;

				private string _title;
				private string _editorPrefsTag;
				private TimelineStateMachineEditorStyle _style;
				private TimelineScrollArea.eTimeFormat _timeFormat;
				private TimelineStateMachineEditorPrefs _editorPrefs;
				private Type[] _allowedEvents;
				
				private enum eMode
				{
					ViewingStateMachine,
					ViewingState
				}
				private string _currentFileName;
				private eMode _currentMode;
				private TimelineEditor _stateEditor;
				private TimelineStateEditorGUI _editedState;
				
				private TimelineStateEditorGUI _lastClickedState;
				private double _lastClickTime;

#if DEBUG
				private TimelineState _playModeHighlightedState = null;
				private bool _debugging = false;
#endif
				#endregion
		
				#region Public Interface
				public void Init(	string title, IEditorWindow editorWindow, string editorPrefsTag,
									Type[] allowedTypes, TimelineStateMachineEditorStyle style,
									TimelineScrollArea.eTimeFormat timeFormat = TimelineScrollArea.eTimeFormat.Seconds)
				{
					_title = title;
					_editorPrefsTag = editorPrefsTag;
					_allowedEvents = allowedTypes;
					_style = style;
					_timeFormat = timeFormat;

					string editorPrefsText = EditorPrefs.GetString(_editorPrefsTag, "");
					_editorPrefs = SerializeConverter.FromString<TimelineStateMachineEditorPrefs>(editorPrefsText);

					if (_editorPrefs == null)
						_editorPrefs = new TimelineStateMachineEditorPrefs();

					SetEditorWindow(editorWindow);
					CreateViews();
				}

				public void UpdateEditor()
				{
#if DEBUG
					if (Application.isPlaying)
					{
						UpdateInPlayMode();
					}
#endif

					if (_stateEditor.NeedsRepaint())
					{
						SetNeedsRepaint();
					}
				}

				public void Render(Vector2 windowSize)
				{
					bool needsRepaint = false;

					EditorGUILayout.BeginVertical();
					{
						RenderToolBar(windowSize);

						switch (_currentMode)
						{
							case eMode.ViewingStateMachine:
								{
									Rect area = new Rect(0.0f, kTopBorder, windowSize.x, windowSize.y - kTopBorder);
									RenderGridView(area);
									needsRepaint = NeedsRepaint();
								}
								break;
							case eMode.ViewingState:
								{
									Rect position = new Rect(0.0f, 53.0f, windowSize.x, windowSize.y - 58);
									needsRepaint = _stateEditor.NeedsRepaint();
									_stateEditor.Render(position);
									needsRepaint |= _stateEditor.NeedsRepaint();
								}
								break;
						}
					}
					EditorGUILayout.EndVertical();

					if (needsRepaint)
						GetEditorWindow().DoRepaint();
				}

				public void OnQuit()
				{
					if (HasChanges() || _stateEditor.HasChanges())
					{
						if (EditorUtility.DisplayDialog("State Machine Has Been Modified", "Do you want to save the changes you made to the state machine:\n\n" + StringUtils.GetAssetPath(_currentFileName) + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
						{
							Save();
						}
					}
				}

				public void Load(string fileName)
				{
					if (ShowOnLoadSaveChangesDialog())
					{
						LoadFile(fileName);
					}
				}

				public void Save()
				{
					if (!string.IsNullOrEmpty(_currentFileName))
					{
						//If we're in state view first need to apply state changes to state machine view
						if (_currentMode == eMode.ViewingState)
						{
							_editedState.GetEditableObject()._timeline = _stateEditor.ConvertToTimeline();
						}

						//Update state machine name to reflect filename
						TimelineStateMachine stateMachine = ConvertToTimelineStateMachine();
						stateMachine._name = System.IO.Path.GetFileNameWithoutExtension(_currentFileName);

						//Save to file
						SerializeConverter.ToFile(stateMachine, _currentFileName);

						ClearDirtyFlag();
						_stateEditor.ClearDirtyFlag();

						GetEditorWindow().DoRepaint();

						//Hack, save string on save scene
						Localisation.SaveStrings();
					}
					else
					{
						SaveAs();
					}
				}

				public void SaveAs()
				{
					string path = EditorUtility.SaveFilePanelInProject("Save state machine", System.IO.Path.GetFileNameWithoutExtension(_currentFileName), "xml", "Please enter a file name to save the state machine to");

					if (!string.IsNullOrEmpty(path))
					{
						_currentFileName = path;
						Save();
					}
				}

				public void New()
				{
					_currentFileName = null;
					_editorPrefs._fileName = null;
					_editorPrefs._stateId = -1;
					SaveEditorPrefs();

					TimelineStateMachine stateMachine = new TimelineStateMachine();
					SetStateMachine(stateMachine);
					_stateEditor.SetTimeline(null);
					SwitchToStatemachineView();

					GetEditorWindow().DoRepaint();
				}

				public void SwitchToStatemachineView()
				{
					if (_editedState != null)
					{
						_editedState.GetEditableObject()._timeline = _stateEditor.ConvertToTimeline();
						_stateEditor.SetTimeline(new Timeline());
						_editedState = null;
					}

					_editorPrefs._stateId = -1;
					SaveEditorPrefs();

					_currentMode = eMode.ViewingStateMachine;
				}

				public void SwitchToStateView(int stateId)
				{
					TimelineStateEditorGUI state = GetTimelineStateGUI(stateId);

					if (state != null)
					{
						_currentMode = eMode.ViewingState;
						_editedState = state;
						_stateEditor.SetTimeline(_editedState.GetEditableObject()._timeline);

						_editorPrefs._stateId = stateId;
						SaveEditorPrefs();

						foreach (TimelineStateEditorGUI stateView in _selectedObjects)
						{
							GetEditorWindow().OnDeselectObject(stateView);
						}
					}
				}

				public void LoadExternalState(TimelineStateEditorGUI state)
				{
					if (ShowOnLoadSaveChangesDialog())
					{
						string fileName = AssetDatabase.GetAssetPath(state.ExternalStateRef._file._editorAsset);
						LoadFile(fileName);
					}
				}

#if DEBUG
				public bool IsDebugging()
				{
					return _debugging;
				}
#endif
				#endregion

				#region TimelineEditorView.IEditor
				public void DoRepaint()
				{
					GetEditorWindow().DoRepaint();
				}

				public void OnSelectObject(ScriptableObject obj)
				{
					GetEditorWindow().OnSelectObject(obj);
				}

				public void OnDeselectObject(ScriptableObject obj)
				{
					GetEditorWindow().OnDeselectObject(obj);
				}

				public void OnAddedNewXmlNode(object obj)
				{
					TimelineStateMachine stateMachine = ConvertToTimelineStateMachine();
					TimelineStateMachine.FixupStateRefs(stateMachine, obj);
				}
				#endregion

				#region EditableObjectGridEditor
				protected override void OnZoomChanged(float zoom)
				{
					SaveEditorPrefs();
				}

				protected override void RenderObjectsOnGrid()
				{				
					_style._stateTextStyle.fontSize = Mathf.RoundToInt(_style._stateTextStyleFontSize * _currentZoom);
					_style._externalStateTextStyle.fontSize = Mathf.RoundToInt(_style._externalStateTextStyleFontSize * _currentZoom);
					_style._linkTextStyle.fontSize = Mathf.RoundToInt(_style._linkTextStyleFontSize * _currentZoom);
					_style._noteTextStyle.fontSize = Mathf.RoundToInt(_style._noteTextStyleFontSize * _currentZoom);


					SetupExternalState();

					List<TimelineStateEditorGUI> toRender = new List<TimelineStateEditorGUI>();
					foreach (TimelineStateEditorGUI editorGUI in _editableObjects)
					{
						if (!editorGUI.IsExternal)
							toRender.Add(editorGUI);
					}					

					foreach (TimelineStateEditorGUI state in toRender)
					{
						
						bool selected = _selectedObjects.Contains(state);
						float borderSize = selected ? state.IsNote ? 1.0f : 2.0f : state.IsNote ? 0.0f : 1.0f;

						Color borderColor = selected ? _style._stateBackgroundSelected : _style._stateBackground;
#if DEBUG
						if (_debugging && _playModeHighlightedState != null && !state.IsNote && state.GetStateId() == _playModeHighlightedState._stateId)
						{
							borderColor = _style._debugCurrentStateColor;
							borderSize = 2.0f;
						}
#endif
						GUIStyle style = state.IsNote ? _style._noteTextStyle : _style._stateTextStyle;

						state.CalcBounds(_style._noteTextStyle, style);
						Rect renderedRect = GetScreenRect(state.GetBounds());

						Color stateColor;
						if (state.GetEditableObject()._editorAutoColor)
							stateColor = state.IsNote ? _style._noteColor : _style._defaultStateColor;
						else
							stateColor = state.GetEditableObject()._editorColor;

						GUIContent stateLabel = state.IsNote ? new GUIContent("Note") :  new GUIContent("State" + state.GetStateId().ToString("00"));

						state.Render(renderedRect, stateLabel, borderColor, stateColor, _style._noteTextStyle, style, borderSize);

						if (!state.IsNote)
							RenderLinksForState(state);
					}

					CleanupExternalState();
				}
				#endregion

				#region EditableObjectEditor
				protected override SerializedObjectEditorGUI<TimelineState> CreateObjectEditorGUI(TimelineState state)
				{
					TimelineStateEditorGUI editorGUI = TimelineStateEditorGUI.CreateInstance<TimelineStateEditorGUI>();
					editorGUI.Init(this, state);
					editorGUI.CalcBounds(_style._noteTextStyle, _style._stateTextStyle);
					return editorGUI;
				}

				protected override void OnCreatedNewObject(TimelineState state)
				{
					bool isNote = state is TimelineStateMachineNote;

					if (isNote)
					{
						state._editorDescription = "New Note";
					}
					else
					{
						if (state._stateId == -1)
							state._stateId = GenerateNewStateId();

						if (string.IsNullOrEmpty(state._editorDescription))
							state._editorDescription = "State" + state._stateId;
					}
				}

				protected override TimelineState CreateCopyFrom(SerializedObjectEditorGUI<TimelineState> editorGUI)
				{
					TimelineStateEditorGUI timeLineGUI = (TimelineStateEditorGUI)editorGUI;
					TimelineState newState = SerializeConverter.CreateCopy(timeLineGUI.GetEditableObject());

					if (timeLineGUI.IsNote)
					{
						newState._editorDescription = timeLineGUI.GetEditorDescrition();
					}
					else
					{
						newState._editorDescription = timeLineGUI.GetEditorDescrition() + " (Copy)";
						newState._stateId = GenerateNewStateId();
					}				

					return newState;
				}

				protected override void SetObjectPosition(SerializedObjectEditorGUI<TimelineState> editorGUI, Vector2 position)
				{
					editorGUI.GetEditableObject()._editorPosition = position;
				}
				
				protected override void OnLeftMouseDown(UnityEngine.Event inputEvent)
				{
					base.OnLeftMouseDown(inputEvent);

					TimelineStateEditorGUI clickedOnState = _draggedObject as TimelineStateEditorGUI;

					if (clickedOnState != null)
					{
						if (_lastClickedState == clickedOnState && (EditorApplication.timeSinceStartup - _lastClickTime) < kDoubleClickTime)
						{
							OnDoubleClickState(clickedOnState as TimelineStateEditorGUI);
						}

						_lastClickedState = clickedOnState;
						_lastClickTime = EditorApplication.timeSinceStartup;
					}
				}

				protected override void AddContextMenu(GenericMenu menu)
				{
					menu.AddItem(new GUIContent("Create New State"), false, AddNewStateMenuCallback);
					menu.AddItem(new GUIContent("Add Note"), false, AddNewNoteMenuCallback);
				}
				#endregion

				#region Private Functions
				private void SaveEditorPrefs()
				{
					string prefsXml = SerializeConverter.ToString(_editorPrefs);
					EditorPrefs.SetString(_editorPrefsTag, prefsXml);
				}

				private TimelineStateMachine ConvertToTimelineStateMachine()
				{
					List<TimelineState> states = new List<TimelineState>();
					List<TimelineStateMachineNote> notes = new List<TimelineStateMachineNote>();

					foreach (TimelineStateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI.IsNote)
							notes.Add((TimelineStateMachineNote)editorGUI.GetEditableObject());
						else if (!editorGUI.IsExternal)
							states.Add(editorGUI.GetEditableObject());
					}

					TimelineStateMachine stateMachine = new TimelineStateMachine();
					stateMachine._states = states.ToArray();
					stateMachine._editorNotes = notes.ToArray();
					return stateMachine;
				}

				private void CreateViews()
				{
					_stateEditor = TimelineEditor.CreateInstance<TimelineEditor>();
					_stateEditor.Init(this, _timeFormat);
					_stateEditor.SetEventTypes(_allowedEvents);

					_currentMode = eMode.ViewingStateMachine;
					_currentFileName = string.Empty;

					_currentZoom = _editorPrefs._zoom;

					if (!string.IsNullOrEmpty(_editorPrefs._fileName))
					{
						LoadFile(_editorPrefs._fileName);
					}
				}

				private void LoadFile(string fileName)
				{
					_currentFileName = fileName;

					TimelineStateMachine stateMachine = SerializeConverter.FromFile<TimelineStateMachine>(fileName);

					if (stateMachine != null)
					{
						if (_editorPrefs._fileName != fileName)
						{
							_editorPrefs._fileName = fileName;
							_editorPrefs._stateId = -1;
							SaveEditorPrefs();
						}

						SetStateMachine(stateMachine);
						_stateEditor.SetTimeline(null);
						
						if (_editorPrefs._stateId != -1)
						{
							SwitchToStateView(_editorPrefs._stateId);
						}
						else
						{
							SwitchToStatemachineView();
						}
					}
					else
					{
						_editorPrefs._fileName = null;
						_editorPrefs._stateId = -1;
						SaveEditorPrefs();
					}

					GetEditorWindow().DoRepaint();
				}

				private bool ShowOnLoadSaveChangesDialog()
				{
					if (HasChanges() || _stateEditor.HasChanges())
					{
						int option = EditorUtility.DisplayDialogComplex("State Machine Has Been Modified", "Do you want to save the changes you made to the state machine:\n\n" + StringUtils.GetAssetPath(_currentFileName) + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save", "Cancel");

						switch (option)
						{
							//Save
							case 0: Save(); return true;
							//Dont save
							case 1: return true;
							//Cancel load
							default:
							case 2: return false;
						}
					}

					return true;
				}

				private void RenderToolBar(Vector2 windowSize)
				{
					EditorGUILayout.BeginVertical();
					{
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							string titleText = _title + " - <b>" + System.IO.Path.GetFileName(_currentFileName);

							if (HasChanges() || _stateEditor.HasChanges())
								titleText += "*";

							titleText += "</b>";

							EditorGUILayout.LabelField(titleText, _style._titleStyle);
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (GUILayout.Button("New", EditorStyles.toolbarButton))
							{
								if (ShowOnLoadSaveChangesDialog())
								{
									New();
								}
							}

							if (GUILayout.Button("Load", EditorStyles.toolbarButton))
							{
								if (ShowOnLoadSaveChangesDialog())
								{
									string fileName = EditorUtility.OpenFilePanel("Open File", Application.dataPath + "/gamedata", "xml");
									if (fileName != null && fileName != string.Empty)
									{
										LoadFile(fileName);
									}
								}
							}

							if (GUILayout.Button("Save", EditorStyles.toolbarButton))
							{
								Save();
							}

							if (GUILayout.Button("Save As", EditorStyles.toolbarButton))
							{
								SaveAs();
							}

							EditorGUILayout.Space();

							_editorPrefs._debug = GUILayout.Toggle(_editorPrefs._debug, "Debug StateMachine", EditorStyles.toolbarButton);

							StateMachine currentDbugObject = _editorPrefs._debugObject.GetComponent();
							StateMachine debugObject = (StateMachine)EditorGUILayout.ObjectField(currentDbugObject, typeof(StateMachine), true);

							if (currentDbugObject != debugObject)
							{
								_editorPrefs._debugObject.SetComponent(debugObject, GameObjectRef.eSourceType.Scene);
								SaveEditorPrefs();
							}


							_editorPrefs._debugLockFocus = GUILayout.Toggle(_editorPrefs._debugLockFocus, "Lock Focus", EditorStyles.toolbarButton);

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (_currentMode == eMode.ViewingState)
							{
								if (GUILayout.Button("Back", EditorStyles.toolbarButton))
								{
									SwitchToStatemachineView();
								}
								else
								{
									EditorGUILayout.Space();

									string stateText = "state" + ((int)(_editedState.GetStateId())).ToString("000") + " - <b>" + StringUtils.GetFirstLine(_editedState.GetEditorDescrition()) + "</b>";
									GUILayout.Toggle(true, stateText, _style._stateTitleStyle);

									GUILayout.FlexibleSpace();
								}
							}
							else
							{
								GUILayout.Button("Zoom", EditorStyles.toolbarButton);

								float zoom = EditorGUILayout.Slider(_currentZoom, 0.5f, 1.5f);

								if (GUILayout.Button("Reset Zoom", EditorStyles.toolbarButton))
								{
									zoom = 1.0f;
								}

								if (_currentZoom != zoom)
								{
									_currentZoom = zoom;

									_editorPrefs._zoom = _currentZoom;
									SaveEditorPrefs();
								}

								if (GUILayout.Button("Center", EditorStyles.toolbarButton))
								{
									CenterCamera();
								}
								GUILayout.FlexibleSpace();
							}
						}

						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
				
				private void SetStateMachine(TimelineStateMachine stateMachine)
				{
					ClearObjects();
#if DEBUG
					_playModeHighlightedState = null;
#endif

					for (int i = 0; i < stateMachine._states.Length; i++)
					{
						AddNewObject(stateMachine._states[i]);
					}

					for (int i = 0; i < stateMachine._editorNotes.Length; i++)
					{
						AddNewObject(stateMachine._editorNotes[i]);
					}

					CenterCamera();
				}

				private TimelineStateEditorGUI GetTimelineStateGUI(int stateId)
				{
					foreach (TimelineStateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI.GetStateId() == stateId)
						{
							return editorGUI;
						}
					}

					return null;
				}
				
				private TimelineState GetTimelineState(int stateId)
				{
					foreach (TimelineStateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI.GetStateId() == stateId)
						{
							return editorGUI.GetEditableObject();
						}
					}

					return null;
				}

				private void RenderExternalState(EditorStateLink link, TimelineStateEditorGUI fromState, float edgeFraction)
				{
					TimelineStateEditorGUI externalState = null;

					//Find external link for this state
					foreach (TimelineStateEditorGUI state in _editableObjects)
					{
						if (state.IsExternal && state.ExternalStateRef._stateId == link._timeline._stateId && state.ExternalStateRef._file._fileGUID == link._timeline._file._fileGUID)
						{
							externalState = state;
							break;
						}
					}

					//If none exists, create a new one
					if (externalState == null)
					{
						externalState = (TimelineStateEditorGUI)AddNewObject(new TimelineState());
						externalState.IsExternal = true;
						externalState.ExternalStateRef = link._timeline;
						_editableObjects.Add(externalState);
					}

					if (!externalState.ExternalHasRendered)
					{
						externalState.CalcBounds(_style._noteTextStyle, _style._externalStateTextStyle);
						bool selected = _selectedObjects.Contains(externalState);
						Color borderColor = selected ? _style._stateBackgroundSelected : _style._stateBackground;
						Rect renderedRect = GetScreenRect(externalState.GetBounds());
						externalState.Render(renderedRect, new GUIContent("External State"), borderColor, _style._externalStateColor, _style._noteTextStyle, _style._externalStateTextStyle, selected ? 2.0f : 1.0f);
						externalState.ExternalHasRendered = true;
					}

					RenderLink(link._description, fromState, externalState, edgeFraction);
				}

				private void RenderLink(string description, TimelineStateEditorGUI fromState, TimelineStateEditorGUI toState, float edgeFraction)
				{
					Rect fromStateRect = GetScreenRect(fromState.GetBounds());
					Rect toStateRect = GetScreenRect(toState.GetBounds());

					Vector3 startPos = new Vector3(fromStateRect.x + fromStateRect.width * edgeFraction, fromStateRect.y + fromStateRect.height - TimelineStateEditorGUI.kShadowSize - 1.0f, 0);
					Vector3 endPos = new Vector3(Mathf.Round(toStateRect.x + toStateRect.width / 2.0f) + 0.5f, Mathf.Round(toStateRect.y - kArrowHeight - 1.0f) + 0.5f, 0);
					Vector3 startTangent = startPos + Vector3.up * 50.0f;
					Vector3 endTangent = endPos - Vector3.up * 50.0f;
					Handles.BeginGUI();
					Handles.color = _style._linkColor;
					Handles.DrawBezier(startPos, endPos, startTangent, endTangent, _style._linkColor, EditorUtils.BezierAATexture, 2f);
					Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(endPos.x, endPos.y + kArrowHeight, 0.0f), new Vector3(endPos.x + kArrowWidth, endPos.y, 0.0f), new Vector3(endPos.x - kArrowWidth, endPos.y, 0.0f) });
					Handles.EndGUI();

					Vector2 textSize = _style._linkTextStyle.CalcSize(new GUIContent(description));
					float lineFraction = edgeFraction;
					Rect labelPos = new Rect(startPos.x + ((endPos.x - startPos.x) * lineFraction) - (textSize.x * 0.5f), startPos.y + ((endPos.y - startPos.y) * lineFraction) - (textSize.y * 0.5f), textSize.x, textSize.y);

					Color origColor = GUI.backgroundColor;

					GUI.backgroundColor = new Color(0.0f, 0.0f, 0.0f, 0.35f);
					GUI.Label(new Rect(labelPos.x + TimelineStateEditorGUI.kShadowSize, labelPos.y + TimelineStateEditorGUI.kShadowSize, labelPos.width, labelPos.height), GUIContent.none, _style._linkTextStyle);

					GUI.backgroundColor = _style._linkDescriptionColor;
					GUI.Label(labelPos, description, _style._linkTextStyle);

					GUI.backgroundColor = origColor;
				}

				private void RenderLinksForState(TimelineStateEditorGUI state)
				{
					Timeline timeline = state.GetEditableObject()._timeline;

					if (timeline != null)
					{
						Event[] events = timeline._events;

						List<EditorStateLink> stateLinks = new List<EditorStateLink>();

						foreach (IStateMachineEvent evnt in events)
						{
							EditorStateLink[] links = evnt.GetEditorLinks();

							if (links != null)
							{
								stateLinks.AddRange(links);
							}
						}

						float fraction = 1.0f / (stateLinks.Count + 1.0f);
						float current = fraction;

						foreach (EditorStateLink link in stateLinks)
						{
							if (link._timeline.IsInternal())
							{
								TimelineStateEditorGUI toState = FindStateForLink(link);

								if (toState != null)
								{
									RenderLink(link._description, state, toState, current);
								}
							}
							else
							{
								RenderExternalState(link, state, current);
							}

							current += fraction;
						}
					}
				}

				private int GenerateNewStateId()
				{
					int stateId = 0;

					foreach (TimelineStateEditorGUI state in _editableObjects)
					{
						stateId = Math.Max(stateId, state.GetStateId() + 1);
					}

					return stateId;
				}

				private TimelineStateEditorGUI FindStateForLink(EditorStateLink link)
				{
					foreach (TimelineStateEditorGUI state in _editableObjects)
					{
						if (link._timeline.GetStateID() == state.GetStateId())
						{
							return state;
						}
					}

					return null;
				}

				private void SetupExternalState()
				{
					foreach (TimelineStateEditorGUI state in _editableObjects)
					{
						state.ExternalHasRendered = false;
					}
				}

				private void CleanupExternalState()
				{
					List<TimelineStateEditorGUI> states = new List<TimelineStateEditorGUI>();
					foreach (TimelineStateEditorGUI editorGUI in _editableObjects)
						states.Add(editorGUI);

					foreach (TimelineStateEditorGUI editorGUI in states)
					{
						if (editorGUI.IsExternal && !editorGUI.ExternalHasRendered)
						{
							RemoveObject(editorGUI);
						}
					}
				}

				private void OnDoubleClickState(TimelineStateEditorGUI state)
				{
					if (!state.IsNote)
					{
						if (state.IsExternal)
						{
							LoadExternalState(state);
						}
						else
						{
							SwitchToStateView(state.GetStateId());
						}
					}
				}

				private void AddNewStateMenuCallback()
				{
					CreateAndAddNewObject(typeof(TimelineState));
				}

				private void AddNewNoteMenuCallback()
				{
					CreateAndAddNewObject(typeof(TimelineStateMachineNote));
				}
#if DEBUG
				private void UpdateInPlayMode()
				{
					_debugging = false;

					if (_editorPrefs._debug)
					{
						StateMachine stateMachine = _editorPrefs._debugObject.GetComponent();
						TimelineStateMachineDebug.StateInfo stateInfo = TimelineStateMachineDebug.GetStateInfo(stateMachine != null ? stateMachine.gameObject : null);

						if (stateInfo != null)
						{
							_debugging = true;

							if (_currentFileName != stateInfo._fileName && stateInfo._fileName != null)
							{
								_currentFileName = stateInfo._fileName;
								SetStateMachine(stateInfo._stateMachine);
								_stateEditor.SetTimeline(null);
							}

							switch (_currentMode)
							{
								case eMode.ViewingStateMachine:
									{
										if (_playModeHighlightedState != stateInfo._state)
											GetEditorWindow().DoRepaint();

										_playModeHighlightedState = stateInfo._state;

										if (_editorPrefs._debugLockFocus)
											CenterCameraOn(GetTimelineStateGUI(_playModeHighlightedState._stateId));
									}
									break;
								case eMode.ViewingState:
									{
										if (stateInfo._state._stateId != _editedState.GetStateId())
										{
											_editedState = GetTimelineStateGUI(stateInfo._state._stateId);
											_stateEditor.SetTimeline(stateInfo._state._timeline);
										}

										_stateEditor.SetPlayModeCursorTime(stateInfo._time);
										GetEditorWindow().DoRepaint();
									}
									break;
							}
						}
					}
				}
#endif
				#endregion
			}
		}
	}
}