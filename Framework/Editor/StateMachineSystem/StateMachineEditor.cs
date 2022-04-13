using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	using System.Reflection;
	using Utils;
	using Utils.Editor;
	
	namespace StateMachineSystem
	{
		namespace Editor
		{
			public sealed class StateMachineEditor : SerializedObjectGridBasedEditor<State>
			{
				#region Private Data
				private static readonly float kDoubleClickTime = 0.32f;

				private string _title;
				private string _editorPrefsTag;
				private StateMachineEditorStyle _style;
				private StateMachineEditorPrefs _editorPrefs;
				
				private string _currentFileName;

				private Type[] _stateTypes;
		
				private StateEditorGUI _lastClickedState;
				private double _lastClickTime;

				private StateEditorGUI _draggingState;
				private StateMachineEditorLink _draggingStateLink;
				private int _draggingStateLinkIndex;
#if DEBUG
				private State _playModeHighlightedState = null;
				private bool _debugging = false;
#endif
				#endregion

				#region Public Interface
				public void Init(string title, IEditorWindow editorWindow, string editorPrefsTag, StateMachineEditorStyle style)
				{
					_title = title;
					_editorPrefsTag = editorPrefsTag;
					_style = style;
					
					string editorPrefsText = ProjectEditorPrefs.GetString(_editorPrefsTag, "");
					try
					{
						_editorPrefs = Serializer.FromString<StateMachineEditorPrefs>(editorPrefsText);
					}
					catch
					{
						_editorPrefs = null;
					}

					if (_editorPrefs == null)
						_editorPrefs = new StateMachineEditorPrefs();

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
				}

				public void Render(Vector2 windowSize)
				{
					bool needsRepaint = false;

					EditorGUILayout.BeginVertical();
					{
						RenderToolBar(windowSize);

						float toolBarHeight = EditorStyles.toolbar.fixedHeight * 3f;
						Rect area = new Rect(0.0f, toolBarHeight, windowSize.x, windowSize.y - toolBarHeight);

						RenderGridView(area);

						needsRepaint = NeedsRepaint();
					}
					EditorGUILayout.EndVertical();

					if (needsRepaint)
						GetEditorWindow().DoRepaint();
				}

				public void OnQuit()
				{
					if (HasChanges())
					{
						if (EditorUtility.DisplayDialog("State Machine Has Been Modified", "Do you want to save the changes you made to the state machine:\n\n" + AssetUtils.GetAssetPath(_currentFileName) + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
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
						//Update state machine name to reflect filename
						StateMachine stateMachine = ConvertToStateMachine();	

						//Save to file
						Serializer.ToFile(stateMachine, _currentFileName);
						
						ClearDirtyFlag();
						
						GetEditorWindow().DoRepaint();
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

					StateMachine stateMachine = new StateMachine();
					SetStateMachine(stateMachine);
					GetEditorWindow().DoRepaint();
				}
				
				public void LoadExternalState(StateMachineExternalStateEditorGUI state)
				{
					if (ShowOnLoadSaveChangesDialog())
					{
						StateRef stateRef = state.ExternalStateRef.GetStateRef();
						string fileName = AssetDatabase.GetAssetPath(stateRef.GetExternalFile()._editorAsset);
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

				#region EditableObjectGridEditor
				protected override void OnZoomChanged(float zoom)
				{
					SaveEditorPrefs();
				}

				protected override void RenderObjectsOnGrid()
				{
					_style._stateLabelStyle.fontSize = Mathf.RoundToInt(_style._stateLabelFontSize * _currentZoom);
					_style._stateTextStyle.fontSize = Mathf.RoundToInt(_style._stateTextFontSize * _currentZoom);
					_style._externalStateTextStyle.fontSize = Mathf.RoundToInt(_style._externalStateTextStyleFontSize * _currentZoom);
					_style._linkTextStyle.fontSize = Mathf.RoundToInt(_style._linkTextFontSize * _currentZoom);
					_style._noteTextStyle.fontSize = Mathf.RoundToInt(_style._noteFontSize * _currentZoom);

					SetupExternalState();

					List<StateEditorGUI> toRender = new List<StateEditorGUI>();
					foreach (StateEditorGUI editorGUI in _editableObjects)
					{
						if (!(editorGUI is StateMachineExternalStateEditorGUI))
							toRender.Add(editorGUI);
					}

					StateEditorGUI dragHighlightState = null;

					//Update state bounds
					foreach (StateEditorGUI state in toRender)
						state.CalcBounds(_style);

					//Check dragging a state link onto a state
					if (_dragMode == eDragType.Custom)
					{
						Vector2 gridPos = GetEditorPosition(Event.current.mousePosition);

						//Check mouse is over a state
						foreach (StateEditorGUI editorGUI in _editableObjects)
						{
							if (editorGUI.GetBounds().Contains(gridPos))
							{
								dragHighlightState = editorGUI;
								break;
							}
						}
					}

					//Render each state
					foreach (StateEditorGUI state in toRender)
					{					
						bool selected = (_dragMode != eDragType.Custom && _selectedObjects.Contains(state)) || dragHighlightState == state;
						float borderSize = state.GetBorderSize(selected);

						Color borderColor = selected ? _style._stateBorderSelectedColor : _style._stateBorderColor;
#if DEBUG
						if (_debugging && _playModeHighlightedState != null && state.GetStateId() == _playModeHighlightedState._stateId)
						{
							borderColor = _style._stateBorderColorDebug;
							borderSize = 2.0f;
						}
#endif
						Rect renderedRect = GetScreenRect(state.GetBounds());
						state.Render(renderedRect, _style, borderColor, borderSize);

						RenderLinksForState(state);
					}

					//Render dragging link
					if (_dragMode == eDragType.Custom)
					{
						Vector3 startPos = GetLinkStartPosition(_draggingState, _draggingStateLinkIndex);
						Vector3 endPos = Event.current.mousePosition + new Vector2(0,-5);

						RenderLinkLine(startPos, endPos, _style._linkColor);
					}


					CleanupExternalState();
				}
				#endregion

				#region EditableObjectEditor
				protected override SerializedObjectEditorGUI<State> CreateObjectEditorGUI(State state)
				{
					StateEditorGUI editorGUI = StateEditorGUI.CreateStateEditorGUI(this, state);
					editorGUI.CalcBounds(_style);
					return editorGUI;
				}

				protected override void OnCreatedNewObject(State state)
				{
					bool isNote = state is StateMachineNote;

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

					//Create a temporary stateMachine and fixUp any StateRefs etc using it
					StateMachine stateMachine = ConvertToStateMachine();
					stateMachine.FixUpStates(stateMachine);
				}

				protected override State CreateCopyFrom(SerializedObjectEditorGUI<State> editorGUI)
				{
					StateEditorGUI timeLineGUI = (StateEditorGUI)editorGUI;
					State newState = Serializer.CreateCopy(timeLineGUI.GetEditableObject());

					newState._editorDescription = timeLineGUI.GetStateDescription() + " (Copy)";
					newState._stateId = GenerateNewStateId();

					return newState;
				}

				protected override void SetObjectPosition(SerializedObjectEditorGUI<State> editorGUI, Vector2 position)
				{
					editorGUI.GetEditableObject()._editorPosition = position;
				}

				protected override void OnLeftMouseDown(Event inputEvent)
				{
					//Dragging state links
					for (int i = 0; i < _editableObjects.Count; i++)
					{
						StateEditorGUI state = (StateEditorGUI)_editableObjects[i];

						StateMachineEditorLink[] links = GetStateLinks(state.GetEditableObject());

						if (links != null)
						{
							float scale = 1.0f;
							float linkRadius = Mathf.Round(_style._linkIconWidth * 0.5f * scale) + 2.0f;

							for (int j=0; j<links.Length; j++)
							{
								Vector3 startPos = GetLinkStartPosition(state, j);
								Vector2 toField = inputEvent.mousePosition - new Vector2(startPos.x, startPos.y);

								if (toField.magnitude < linkRadius)
								{
									_dragMode = eDragType.Custom;
									_draggingState = state;
									_draggingStateLink = links[j];
									_draggingStateLinkIndex = j;
									_dragPos = inputEvent.mousePosition;
									_dragAreaRect = new Rect(-1.0f, -1.0f, 0.0f, 0.0f);
									_selectedObjects.Clear();
									return;
								}
							}
						}
					}

					//Normal input
					base.OnLeftMouseDown(inputEvent);

					//Double clicking
					StateEditorGUI clickedOnState = _draggedObject as StateEditorGUI;

					if (clickedOnState != null)
					{
						if (_lastClickedState == clickedOnState && (EditorApplication.timeSinceStartup - _lastClickTime) < kDoubleClickTime)
						{
							OnDoubleClickState(clickedOnState);
						}

						_lastClickedState = clickedOnState;
						_lastClickTime = EditorApplication.timeSinceStartup;
					}
				}

				protected override void OnDragging(Event inputEvent)
				{
					if (_dragMode == eDragType.Custom)
					{
						SetNeedsRepaint();
					}
					else
					{
						base.OnDragging(inputEvent);
					}
				}

				protected override void OnStopDragging(Event inputEvent, bool cancelled)
				{
					if (_dragMode == eDragType.Custom)
					{
						if (!cancelled)
						{
							Vector2 gridPos = GetEditorPosition(Event.current.mousePosition);

							StateEditorGUI draggedOnToState = null;

							//Check mouse is over a state
							foreach (StateEditorGUI editorGUI in _editableObjects)
							{
								if (editorGUI.GetBounds().Contains(gridPos))
								{
									draggedOnToState = editorGUI;

									// Check its moved more than 
									break;
								}
							}

							if (draggedOnToState != null)
							{
								_draggingStateLink.SetStateRef(new StateRef(draggedOnToState.GetStateId()));
							}
							else
							{
								_draggingStateLink.SetStateRef(new StateRef());
							}
						}

						inputEvent.Use();
						_dragMode = eDragType.NotDragging;

						_draggingState = null;
						_draggingStateLink = new StateMachineEditorLink();
						_draggingStateLinkIndex = 0;
					}
					else
					{
						base.OnStopDragging(inputEvent, cancelled);
					}
				}

				protected override void AddContextMenu(GenericMenu menu)
				{
					if (_stateTypes == null)
					{
						List<Type> stateTypes = new List<Type>(SystemUtils.GetAllSubTypes(typeof(State)));

						stateTypes.Remove(typeof(CoroutineState));
						stateTypes.Remove(typeof(PlayableGraphState));
						stateTypes.Remove(typeof(StateMachineNote));
						stateTypes.Remove(typeof(StateMachineExternalState));


						_stateTypes = stateTypes.ToArray();
					}

					menu.AddItem(new GUIContent("Add Note"), false, AddNewStateMenuCallback, typeof(StateMachineNote));
					menu.AddItem(new GUIContent("Add Coroutine State"), false, AddNewStateMenuCallback, typeof(CoroutineState));
					menu.AddItem(new GUIContent("Add Playable Graph State"), false, AddNewStateMenuCallback, typeof(PlayableGraphState));

					menu.AddSeparator(null);

					for (int i=0; i<_stateTypes.Length; i++)
					{
						menu.AddItem(new GUIContent("Add " + _stateTypes[i].Name), false, AddNewStateMenuCallback, _stateTypes[i]);
					}
				}
				#endregion

				#region Private Functions
				private void SaveEditorPrefs()
				{
					string prefsXml = Serializer.ToString(_editorPrefs);
					ProjectEditorPrefs.SetString(_editorPrefsTag, prefsXml);
				}

				private StateMachine ConvertToStateMachine()
				{
					List<State> states = new List<State>();
					List<StateMachineNote> notes = new List<StateMachineNote>();

					foreach (StateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI is StateMachineNoteEditorGUI)
							notes.Add((StateMachineNote)editorGUI.GetEditableObject());
						else if (!(editorGUI is StateMachineExternalStateEditorGUI))
							states.Add(editorGUI.GetEditableObject());
					}

					StateMachine stateMachine = new StateMachine();
					stateMachine._states = states.ToArray();
					stateMachine._editorNotes = notes.ToArray();
					stateMachine._name = System.IO.Path.GetFileNameWithoutExtension(_currentFileName);

					return stateMachine;
				}

				private void CreateViews()
				{
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

					StateMachine stateMachine = null;

					try
					{
						stateMachine = Serializer.FromFile<StateMachine>(fileName);
					}
					catch (ObjectNotFoundException)
					{
						EditorUtility.DisplayDialog("StateMachine Editor", AssetUtils.GetAssetPath(_currentFileName) + " does not contain a valid StateMachine.", "Ok");
					}
					catch (CorruptFileException)
					{
						EditorUtility.DisplayDialog("StateMachine Editor", AssetUtils.GetAssetPath(_currentFileName) + " is corrupt.", "Ok");
					}


					if (stateMachine != null)
					{
						if (_editorPrefs._fileName != fileName)
						{
							_editorPrefs._fileName = fileName;
							_editorPrefs._stateId = -1;
							SaveEditorPrefs();
						}

						SetStateMachine(stateMachine);
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
					if (HasChanges())
					{
						int option = EditorUtility.DisplayDialogComplex("State Machine Has Been Modified", "Do you want to save the changes you made to the state machine:\n\n" + AssetUtils.GetAssetPath(_currentFileName) + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save", "Cancel");

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

							if (HasChanges())
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

							StateMachineComponent currentDbugObject = _editorPrefs._debugObject.GetComponent();
							StateMachineComponent debugObject = (StateMachineComponent)EditorGUILayout.ObjectField(currentDbugObject, typeof(StateMachineComponent), true);

							if (currentDbugObject != debugObject)
							{
								_editorPrefs._debugObject = new ComponentRef<StateMachineComponent>(GameObjectRef.eSourceType.Scene, debugObject);

								if (debugObject != null && _editorPrefs._debugObject.GetComponent() == null)
								{
									_editorPrefs._debugObject = new ComponentRef<StateMachineComponent>(GameObjectRef.eSourceType.Prefab, debugObject);
								}

								SaveEditorPrefs();
							}

							_editorPrefs._debugLockFocus = GUILayout.Toggle(_editorPrefs._debugLockFocus, "Lock Focus", EditorStyles.toolbarButton);

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
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

						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
				
				private void SetStateMachine(StateMachine stateMachine)
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

				private StateEditorGUI GetStateGUI(int stateId)
				{
					foreach (StateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI.GetStateId() == stateId)
						{
							return editorGUI;
						}
					}

					return null;
				}
				
				private void RenderExternalLink(StateMachineEditorLink link, StateEditorGUI fromState, int linkIndex)
				{
					StateMachineExternalStateEditorGUI externalState = null;

					StateRef stateRef = link.GetStateRef();

					//Find external link for this state
					foreach (StateEditorGUI state in _editableObjects)
					{
						StateMachineExternalStateEditorGUI extState = state as StateMachineExternalStateEditorGUI;

						if (extState != null)
						{						
							StateRef extStateRef = extState.ExternalStateRef.GetStateRef();

							if (extStateRef.GetStateID() == stateRef.GetStateID() && extStateRef.GetExternalFile().GetFileGUID() == stateRef.GetExternalFile().GetFileGUID())
							{
								externalState = extState;
								break;
							}
						}
						
					}

					//If none exists, create a new one
					if (externalState == null)
					{
						externalState = (StateMachineExternalStateEditorGUI)AddNewObject(new StateMachineExternalState());
						externalState.ExternalStateRef = link;
						_editableObjects.Add(externalState);
					}

					if (!externalState.ExternalHasRendered)
					{
						externalState.CalcBounds(_style);
						bool selected = _selectedObjects.Contains(externalState);
						Color borderColor = selected ? _style._stateBorderSelectedColor : _style._stateBorderColor;
						Rect renderedRect = GetScreenRect(externalState.GetBounds());
						externalState.Render(renderedRect, _style, borderColor, selected ? 2.0f : 1.0f);
						externalState.ExternalHasRendered = true;
					}

					RenderLink(link._description, fromState, externalState, linkIndex);
				}

				private Vector3 GetLinkStartPosition(StateEditorGUI state, int linkIndex = 0)
				{
					float fraction = 1.0f / (GetStateLinks(state.GetEditableObject()).Length + 1.0f);
					float edgeFraction = fraction * (1 + linkIndex);

					Rect stateRect = GetScreenRect(state.GetBounds());
					return new Vector3(Mathf.Round(stateRect.x + stateRect.width * edgeFraction), Mathf.Round(stateRect.y + stateRect.height - _style._shadowSize - 1.0f), 0);
				}

				private Vector3 GetLinkEndPosition(StateEditorGUI state, int linkIndex = 0)
				{
					Rect stateRect = GetScreenRect(state.GetBounds());
					return new Vector3(Mathf.Round(stateRect.x + stateRect.width / 2.0f) + 0.5f, Mathf.Round(stateRect.y - _style._linkArrowHeight - 1.0f) + 0.5f, 0);
				}

				private static readonly float kLineTangent = 50.0f;


				private void RenderLinkLine(Vector3 startPos, Vector3 endPos, Color color)
				{
					Vector3 line = endPos - startPos;
					float lineLength = line.magnitude;

					float tangent = kLineTangent;

					if (lineLength < tangent * 2.0f)
						tangent = lineLength * 0.5f;

					Vector3 startTangent = startPos + Vector3.up * tangent;
					Vector3 endTangent = endPos - Vector3.up * tangent;
					Handles.BeginGUI();
					Handles.color = color;
					Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color, EditorUtils.BezierAATexture, _style._lineLineWidth);
					Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(endPos.x, endPos.y + _style._linkArrowHeight, 0.0f), new Vector3(endPos.x + _style._linkArrowWidth, endPos.y, 0.0f), new Vector3(endPos.x - _style._linkArrowWidth, endPos.y, 0.0f) });
					Handles.EndGUI();
				}

				private void RenderLink(string description, StateEditorGUI fromState, StateEditorGUI toState, int linkIndex)
				{
					Vector3 startPos = GetLinkStartPosition(fromState, linkIndex);
					RenderLinkIcon(startPos, fromState.GetEditableObject().GetEditorColor());

					if (toState != null)
					{
						Vector3 endPos = GetLinkEndPosition(toState, linkIndex);

						RenderLinkLine(startPos, endPos, _dragMode == eDragType.Custom ? _style._linkInactiveColor : _style._linkColor);

						Vector2 textSize = _style._linkTextStyle.CalcSize(new GUIContent(description));
						float lineFraction = 0.5f;
						Rect labelPos = new Rect(startPos.x + ((endPos.x - startPos.x) * lineFraction) - (textSize.x * 0.5f), startPos.y + ((endPos.y - startPos.y) * lineFraction) - (textSize.y * 0.5f), textSize.x, textSize.y);

						//Draw shadow
						Rect shadowRect = new Rect(labelPos.x + _style._shadowSize, labelPos.y + _style._shadowSize, labelPos.width, labelPos.height);
						EditorUtils.DrawColoredRoundedBox(shadowRect, _style._shadowColor, _style._stateCornerRadius);

						//Draw label background
						EditorUtils.DrawColoredRoundedBox(labelPos, _style._linkDescriptionColor, _style._stateCornerRadius);

						//Draw label
						GUI.Label(labelPos, description, _style._linkTextStyle);
					}
				}

				private void RenderLinkIcon(Vector2 position, Color color)
				{
					Vector3 position3d = new Vector3(position.x, position.y, 0.0f);

					float scale = 1.0f;
					float linkRadius = Mathf.Round(_style._linkIconWidth * 0.5f * scale);

					bool highlighted = false;

					if (_dragMode == eDragType.NotDragging)
					{
						Vector2 toField = UnityEngine.Event.current.mousePosition - position;
						highlighted = toField.magnitude < linkRadius + 2.0f;
						SetNeedsRepaint();
					}

					Handles.BeginGUI();

					if (highlighted)
					{
						Handles.color = Color.green;
						Handles.DrawSolidDisc(position3d, -Vector3.forward, linkRadius + 2.0f);
					}

					Handles.color = Color.Lerp(_style._stateBorderColor, Color.clear, 0.25f);
					Handles.DrawSolidDisc(position3d, -Vector3.forward, linkRadius + 1.5f);
					
					Handles.color = color;
					Handles.DrawSolidDisc(position3d, -Vector3.forward, linkRadius);
					Handles.color = Color.Lerp(color, Color.black, 0.5f);
					Handles.DrawSolidDisc(position3d, -Vector3.forward, linkRadius - 2.0f);

					Handles.EndGUI();
					}

				private void RenderLinksForState(StateEditorGUI state)
				{
					StateMachineEditorLink[] links = GetStateLinks(state.GetEditableObject());

					if (links != null)
					{
						for (int j = 0; j < links.Length; j++)
						{
							StateRef stateRef = links[j].GetStateRef();

							if (stateRef.IsInternal())
							{
								StateEditorGUI toState = FindStateForLink(stateRef);
								RenderLink(links[j]._description, state, toState, j);
							}
							else
							{
								RenderExternalLink(links[j], state, j);
							}
						}
					}				
				}

				private int GenerateNewStateId()
				{
					int stateId = 1;

					foreach (StateEditorGUI state in _editableObjects)
					{
						stateId = Math.Max(stateId, state.GetStateId() + 1);
					}

					return stateId;
				}

				private StateEditorGUI FindStateForLink(StateRef link)
				{
					int stateId = link.GetStateID();

					if (stateId != -1)
					{
						foreach (StateEditorGUI state in _editableObjects)
						{
							if (stateId == state.GetStateId())
							{
								return state;
							}
						}
					}

					return null;
				}

				private void SetupExternalState()
				{
					foreach (StateEditorGUI state in _editableObjects)
					{
						StateMachineExternalStateEditorGUI extState = state as StateMachineExternalStateEditorGUI;

						if (extState != null )
							extState.ExternalHasRendered = false;
					}
				}

				private void CleanupExternalState()
				{
					List<StateEditorGUI> states = new List<StateEditorGUI>();
					foreach (StateEditorGUI editorGUI in _editableObjects)
						states.Add(editorGUI);

					foreach (StateEditorGUI editorGUI in states)
					{
						StateMachineExternalStateEditorGUI extState = editorGUI as StateMachineExternalStateEditorGUI;

						if (extState != null && !extState.ExternalHasRendered)
						{
							RemoveObject(editorGUI);
						}
					}
				}

				private void OnDoubleClickState(StateEditorGUI state)
				{
					state.OnDoubleClick();
				}

				private void AddNewStateMenuCallback(object type)
				{				 
					CreateAndAddNewObject((Type)type);
				}

				private StateMachineEditorLink[] GetStateLinks(State state)
				{
					//Loop over member infos in state finding attrbute
					FieldInfo[] feilds = state.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
					List<StateMachineEditorLink> links = new List<StateMachineEditorLink>();

					for (int i = 0; i < feilds.Length; i++)
					{
						StateLinkAttribute attribute = SystemUtils.GetAttribute<StateLinkAttribute>(feilds[i]);

						if (attribute != null)
						{
							StateMachineEditorLink link = new StateMachineEditorLink
							{
								_object = state,
								_memberInfo = new SerializedObjectMemberInfo(feilds[i]),
								_description = attribute._editorName
							};

							links.Add(link);
						}
					}


					return links.ToArray();
				}

#if DEBUG
				private void UpdateInPlayMode()
				{
					_debugging = false;

					if (_editorPrefs._debug)
					{
						StateMachineComponent stateMachine = _editorPrefs._debugObject.GetComponent();
						StateMachineDebug.StateInfo stateInfo = StateMachineDebug.GetStateInfo(stateMachine != null ? stateMachine.gameObject : null);

						if (stateInfo != null)
						{
							_debugging = true;

							if (_currentFileName != stateInfo._fileName && stateInfo._fileName != null)
							{
								_currentFileName = stateInfo._fileName;
								SetStateMachine(stateInfo._stateMachine);
							}

							if (_playModeHighlightedState != stateInfo._state)
								GetEditorWindow().DoRepaint();

							_playModeHighlightedState = stateInfo._state;

							if (_editorPrefs._debugLockFocus)
								CenterCameraOn(GetStateGUI(_playModeHighlightedState._stateId));
						}
					}
				}
#endif
				#endregion
			}
		}
	}
}