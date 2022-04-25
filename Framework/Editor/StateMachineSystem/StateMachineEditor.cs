﻿using System;
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
						AssetDatabase.CreateAsset(stateMachine, _currentFileName);
						
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
					string path = EditorUtility.SaveFilePanelInProject("Save state machine", System.IO.Path.GetFileNameWithoutExtension(_currentFileName), "asset", "Please enter a file name to save the state machine to");

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
					_style._linkTextStyle.fontSize = Mathf.RoundToInt(_style._linkTextFontSize * _currentZoom);
					_style._noteTextStyle.fontSize = Mathf.RoundToInt(_style._noteFontSize * _currentZoom);

					StateEditorGUI dragHighlightState = null;

					//Update state bounds
					foreach (StateEditorGUI state in _editableObjects)
						state.CalcRenderRect(_style);

					//Check dragging a state link onto a state
					if (_dragMode == DragType.Custom)
					{
						Vector2 gridPos = GetEditorPosition(Event.current.mousePosition);

						//Check mouse is over a state
						foreach (StateEditorGUI editorGUI in _editableObjects)
						{
							if (CanDragLinkTo(editorGUI) && editorGUI.GetBounds().Contains(gridPos))
							{
								dragHighlightState = editorGUI;
								break;
							}
						}
					}

					//Render each state
					foreach (StateEditorGUI state in _editableObjects)
					{					
						bool selected = (_dragMode != DragType.Custom && _selectedObjects.Contains(state)) || dragHighlightState == state;
						float borderSize = 2f;

						if (state.GetEditableObject() is StateMachineNote)
						{
							borderSize = selected ? 1f : 0f;
						}

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

						RenderLinksForState(state, selected);
					}

					//Render dragging link
					if (_dragMode == DragType.Custom)
					{
						Vector3 startPos = GetLinkStartPosition(_draggingState, _draggingStateLinkIndex);
						Vector3 endPos = Event.current.mousePosition + new Vector2(0,-5);

						RenderLinkLine(startPos, endPos, _style._linkColor);
					}
				}
				#endregion

				#region EditableObjectEditor
				protected override bool CanBeCopied(SerializedObjectEditorGUI<State> editorGUI)
				{
					return !(editorGUI.GetEditableObject() is StateMachineEntryState);
				}

				protected override bool CanBeDeleted(SerializedObjectEditorGUI<State> editorGUI)
				{
					return !(editorGUI.GetEditableObject() is StateMachineEntryState);
				}

				protected override SerializedObjectEditorGUI<State> CreateObjectEditorGUI(State state)
				{
					StateEditorGUI editorGUI = StateEditorGUI.CreateStateEditorGUI(this, state);
					editorGUI.CalcRenderRect(_style);
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

						StateMachineEditorLink[] links = state.GetEditableObject().GetEditorStateLinks();

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
									_dragMode = DragType.Custom;
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
					if (_dragMode == DragType.Custom)
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
					if (_dragMode == DragType.Custom)
					{
						if (!cancelled)
						{
							Vector2 gridPos = GetEditorPosition(Event.current.mousePosition);

							StateEditorGUI draggedOnToState = null;

							//Check mouse is over a state
							foreach (StateEditorGUI editorGUI in _editableObjects)
							{
								if (CanDragLinkTo(editorGUI) && editorGUI.GetBounds().Contains(gridPos))
								{
									draggedOnToState = editorGUI;
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
						_dragMode = DragType.NotDragging;

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
						stateTypes.Remove(typeof(StateMachineEntryState));

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
					StateMachine stateMachine = new StateMachine();
					stateMachine._name = System.IO.Path.GetFileNameWithoutExtension(_currentFileName);

					List<State> states = new List<State>();
					List<StateMachineNote> notes = new List<StateMachineNote>();

					foreach (StateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI.GetEditableObject() is StateMachineEntryState entryState)
						{
							stateMachine._entryState = entryState;
						}			
						else if(editorGUI.GetEditableObject() is StateMachineNote stateMachineNote)
						{
							notes.Add(stateMachineNote);
						}
						else
						{
							states.Add(editorGUI.GetEditableObject());
						}
					}

					stateMachine._states = states.ToArray();
					stateMachine._editorNotes = notes.ToArray();
					
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

					StateMachine stateMachine = AssetDatabase.LoadAssetAtPath<StateMachine>(AssetUtils.GetAssetPath(fileName));

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
									string fileName = EditorUtility.OpenFilePanel("Open File", Application.dataPath, "asset");
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
								_editorPrefs._debugObject = new ComponentRef<StateMachineComponent>(GameObjectRef.SourceType.Scene, debugObject);
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

					if (stateMachine._entryState == null)
					{
						stateMachine._entryState = new StateMachineEntryState();
					}

					AddNewObject(stateMachine._entryState);

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
				
				private Vector3 GetLinkStartPosition(StateEditorGUI state, int linkIndex = 0)
				{
					float fraction = 1.0f / ((state.GetEditableObject().GetEditorStateLinks()).Length + 1.0f);
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

				private void RenderLinkLabel(Rect labelPos, string text)
				{
					//Draw shadow
					Rect shadowRect = new Rect(labelPos.x + _style._shadowSize, labelPos.y + _style._shadowSize, labelPos.width, labelPos.height);
					EditorUtils.DrawColoredRoundedBox(shadowRect, _style._shadowColor, _style._stateCornerRadius);

					//Draw label background
					EditorUtils.DrawColoredRoundedBox(labelPos, _style._linkDescriptionColor, _style._stateCornerRadius);

					//Draw label
					GUI.Label(labelPos, text, _style._linkTextStyle);
				}

				private void RenderLink(string description, StateEditorGUI fromState, StateEditorGUI toState, int linkIndex, bool selected)
				{
					Vector3 startPos = GetLinkStartPosition(fromState, linkIndex);

					if (toState == null)
					{
						Vector2 textSize = _style._linkTextStyle.CalcSize(new GUIContent(description));
						Rect labelPos = new Rect(startPos.x - (textSize.x * 0.5f), startPos.y + (textSize.y * 0.5f), textSize.x, textSize.y);

						RenderLinkLabel(labelPos, description);
					}

					RenderLinkIcon(fromState, startPos, fromState.GetEditableObject().GetEditorColor(), selected);

					if (toState != null)
					{
						Vector3 endPos = GetLinkEndPosition(toState, linkIndex);

						RenderLinkLine(startPos, endPos, _dragMode == DragType.Custom ? _style._linkInactiveColor : _style._linkColor);

						Vector2 textSize = _style._linkTextStyle.CalcSize(new GUIContent(description));
						float lineFraction = 0.5f;
						Rect labelPos = new Rect(startPos.x + ((endPos.x - startPos.x) * lineFraction) - (textSize.x * 0.5f), startPos.y + ((endPos.y - startPos.y) * lineFraction) - (textSize.y * 0.5f), textSize.x, textSize.y);

						RenderLinkLabel(labelPos, description);
					}
				}

				private void Draw2DCircle(Vector2 position, float radius, Color color)
				{
					EditorUtils.DrawColoredRoundedBox(new Rect(position.x - radius, position.y - radius, radius + radius, radius + radius), color, radius);
				}

				private void RenderLinkIcon(StateEditorGUI state, Vector2 position, Color color, bool selected)
				{
					float scale = 1.0f;
					float linkRadius = Mathf.Round(_style._linkIconWidth * 0.5f * scale);

					bool highlighted = false;

					if (_dragMode == DragType.NotDragging)
					{
						Vector2 toField = Event.current.mousePosition - position;
						highlighted = toField.magnitude < linkRadius + 2.0f;
						SetNeedsRepaint();
					}

					//Border
					if (highlighted)
					{
						Draw2DCircle(position, linkRadius + 2f, Color.green);
					}
					else if (selected)
					{
						Draw2DCircle(position, linkRadius + 2f, _style._stateBorderSelectedColor);
					}
					else
					{
						Draw2DCircle(position, linkRadius + 2f, _style._stateBorderColor);
					}

					//Main
					Draw2DCircle(position, linkRadius, color);

					//Hole
					Draw2DCircle(position, linkRadius - 2.0f, Color.Lerp(color, Color.black, 0.5f));
				}

				private void RenderLinksForState(StateEditorGUI state, bool selected)
				{
					StateMachineEditorLink[] links = state.GetEditableObject().GetEditorStateLinks();

					if (links != null)
					{
						for (int j = 0; j < links.Length; j++)
						{
							StateRef stateRef = links[j].GetStateRef();
							StateEditorGUI toState = FindStateForLink(stateRef);

							RenderLink(links[j]._label, state, toState, j, selected);
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

				private void OnDoubleClickState(StateEditorGUI state)
				{
					state.OnDoubleClick();
				}

				private void AddNewStateMenuCallback(object type)
				{				 
					CreateAndAddNewObject((Type)type);
				}

				private bool CanDragLinkTo(StateEditorGUI editorGUI)
				{
					return !(editorGUI.GetEditableObject() is StateMachineEntryState || editorGUI.GetEditableObject() is StateMachineNote);
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