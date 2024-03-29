﻿using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Serialization;
	using Utils;
	using Utils.Editor;
	
	namespace StateMachineSystem
	{
		namespace Editor
		{
			public sealed class StateMachineEditor : ScriptableObjectHierarchyGridEditor<StateMachine, State>
			{
				#region Private Data
				private static readonly float kLineTangent = 30.0f;

				private string _title;
				private string _editorPrefsTag;
				private StateMachineEditorStyle _style;
				private StateMachineEditorPrefs _editorPrefs;
				
				private Type[] _stateTypes;
		
				private StateEditorGUI _lastClickedState;
				private double _lastClickTime;

				private enum StateLinkDragMode
				{ 
					Link,
					Label,
					LabelFlipped,
				}
				private StateLinkDragMode _stateLinkDragMode;
				private StateEditorGUI _draggingState;
				private StateMachineEditorLink _draggingStateLink;
				private int _draggingStateLinkIndex;

				private enum LinkArrowDirection
				{
					Down,
					Left,
					Right
				}

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
						if (EditorUtility.DisplayDialog("State Machine Has Been Modified", "Do you want to save the changes you made to the state machine:\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
						{
							Save();
						}
					}
				}

				public void SaveAs()
				{
					string path = EditorUtility.SaveFilePanelInProject("Save state machine", Asset.name, "asset", "Please enter a file name to save the state machine to");

					if (!string.IsNullOrEmpty(path))
					{
						SaveAs(path);
					}
				}

				public void New()
				{
					if (ShowOnLoadSaveChangesDialog())
					{
						string path = EditorUtility.SaveFilePanelInProject("New State Machine", "StateMachine", "asset", "Please enter a file name to save the state machine to");

						if (!string.IsNullOrEmpty(path))
						{
							_editorPrefs._fileName = path;
							_editorPrefs._stateId = -1;
							SaveEditorPrefs();

							Create(path);
						}
						else
						{
							_editorPrefs._fileName = null;
							_editorPrefs._stateId = -1;
							SaveEditorPrefs();

							ClearAsset();
						}
					}
				}

#if DEBUG
				public bool IsDebugging()
				{
					return _debugging;
				}
#endif
				#endregion

				#region ScriptableObjectHierarchyGridEditor
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
						bool selected;

						if (_dragMode == DragType.Custom)
						{
							selected = _draggingState == null ? false : dragHighlightState == state; 
						}
						else
						{
							selected = _selectedObjects.Contains(state);
						}

						float borderSize = 2f;

						if (state.Asset is StateMachineNote)
						{
							borderSize = selected ? 1f : 0f;
						}

						Color borderColor = selected ? _style._stateBorderSelectedColor : _style._stateBorderColor;
#if DEBUG
						if (_debugging && _playModeHighlightedState != null && state.Asset == _playModeHighlightedState)
						{
							borderColor = _style._stateBorderColorDebug;
							borderSize = 2.0f;
						}
#endif
						Rect renderedRect = GetScreenRect(state.GetBounds());
						state.Render(renderedRect, _style, borderColor, borderSize);

						StateMachineEditorLink[] links = state.Asset.GetEditorStateLinks();

						if (links != null)
						{
							for (int j = 0; j < links.Length; j++)
							{
								StateRef stateRef = links[j].GetStateRef();
								StateEditorGUI toState = FindStateForLink(stateRef);
								Vector2 startPos = GetLinkStartPosition(state, j);

								if (toState == null)
								{
									RenderLinkLabel(startPos + new Vector2(0f, _style._linkIconWidth * 1.75f), links[j]._label, 0f);
								}

								RenderLinkIcon(state, startPos, state.Asset.GetEditorColor(), selected);

								if (toState != null)
								{
									Vector2 endPos = GetLinkEndPosition(state, startPos, toState, out LinkArrowDirection linkArrowDirection);
									Vector2 labelOffset = stateRef._editorPosition;

									if (linkArrowDirection == LinkArrowDirection.Right)
										labelOffset.x = -labelOffset.x;

									Vector2 labelPos = GetDefaultLinkLabelPos(startPos, endPos) + labelOffset * _currentZoom;

									bool linkActive = true;
									float labelBorder = 0f;

									if (_dragMode == DragType.Custom)
									{
										if ((_stateLinkDragMode == StateLinkDragMode.Label || _stateLinkDragMode == StateLinkDragMode.LabelFlipped) && _draggingState == state && _draggingStateLinkIndex == j)
										{
											linkActive = true;
											labelBorder = borderSize;
										}
										else
										{
											linkActive = false;
										}
									}

									RenderLinkLine(startPos, endPos, labelPos, linkActive ? _style._linkColor : _style._linkInactiveColor, state == toState, linkArrowDirection);
									RenderLinkLabel(labelPos, links[j]._label, labelBorder);
								}
							}
						}
					}

					//Render dragging link
					if (_dragMode == DragType.Custom && _stateLinkDragMode == StateLinkDragMode.Link)
					{
						Vector2 startPos = GetLinkStartPosition(_draggingState, _draggingStateLinkIndex);
						Vector2 endPos = Event.current.mousePosition + new Vector2(0,-5);
						Vector2 labelPos = GetDefaultLinkLabelPos(startPos, endPos);

						RenderLinkLine(startPos, endPos, labelPos, _style._linkColor);
					}
				}

				protected override bool CanBeCopied(ScriptableObjectHierarchyEditorObjectGUI<StateMachine, State> editorGUI)
				{
					return !(editorGUI.Asset is StateMachineEntryState);
				}

				protected override bool CanBeDeleted(ScriptableObjectHierarchyEditorObjectGUI<StateMachine, State> editorGUI)
				{
					return !(editorGUI.Asset is StateMachineEntryState);
				}

				protected override ScriptableObjectHierarchyEditorObjectGUI<StateMachine, State> CreateObjectEditorGUI(State state)
				{
					StateEditorGUI editorGUI = StateEditorGUI.CreateStateEditorGUI(this, state);
					editorGUI.CalcRenderRect(_style);
					return editorGUI;
				}

				protected override void OnCreatedNewObject(State state)
				{
					if (state is StateMachineEntryState)
					{
						state.name = "Entry State";
					}
					else if(state is StateMachineNote)
					{
						int stateId = GenerateNewNoteId();

						state.name = "Note_" + stateId.ToString("000");
						state._editorDescription = "New Note";
					}
					else
					{
						int stateId = GenerateNewStateId();

						state.name = "State_" + stateId.ToString("000");

						if (string.IsNullOrEmpty(state._editorDescription))
							state._editorDescription = "State" + stateId;
					}

					state.SetParent(Asset);
				}

				protected override void OnLeftMouseDown(Event inputEvent)
				{
					//Dragging state links
					for (int i = 0; i < _editableObjects.Count; i++)
					{
						StateEditorGUI state = (StateEditorGUI)_editableObjects[i];

						StateMachineEditorLink[] links = state.Asset.GetEditorStateLinks();

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
									_stateLinkDragMode = StateLinkDragMode.Link;
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

					//Dragging state links
					for (int i = 0; i < _editableObjects.Count; i++)
					{
						StateEditorGUI state = (StateEditorGUI)_editableObjects[i];

						StateMachineEditorLink[] links = state.Asset.GetEditorStateLinks();

						if (links != null)
						{
							for (int j = 0; j < links.Length; j++)
							{
								StateRef stateRef = links[j].GetStateRef();
								StateEditorGUI toState = FindStateForLink(stateRef);

								if (toState != null)
								{
									Vector2 startPos = GetLinkStartPosition(state, j);
									Vector2 endPos = GetLinkEndPosition(state, startPos, toState, out LinkArrowDirection linkArrowDirection);

									Vector2 labelOffset = stateRef._editorPosition;

									if (linkArrowDirection == LinkArrowDirection.Right)
										labelOffset.x = -labelOffset.x;

									Vector2 labelPos = GetDefaultLinkLabelPos(startPos, endPos) + labelOffset * _currentZoom;

									Vector2 labelSize = _style._linkTextStyle.CalcSize(new GUIContent(links[j]._label));
									Rect labelRect = new Rect(labelPos.x - (labelSize.x * 0.5f), labelPos.y - (labelSize.y * 0.5f), labelSize.x, labelSize.y);

									if (labelRect.Contains(inputEvent.mousePosition))
									{
										_dragMode = DragType.Custom;
										_stateLinkDragMode = linkArrowDirection == LinkArrowDirection.Right ? StateLinkDragMode.LabelFlipped : StateLinkDragMode.Label;
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
						Vector2 currentPos = inputEvent.mousePosition;
						Vector2 delta = currentPos - _dragPos;
						_dragPos = currentPos;

						if (_stateLinkDragMode == StateLinkDragMode.Label || _stateLinkDragMode == StateLinkDragMode.LabelFlipped)
						{
							delta *= (1.0f / _currentZoom);

							if (_stateLinkDragMode == StateLinkDragMode.LabelFlipped)
								delta.x = -delta.x;

							//Move label by position
							StateRef stateRef = _draggingStateLink.GetStateRef();
							stateRef._editorPosition += delta;
							SetStateLink(_draggingState.Asset, _draggingStateLink, stateRef);
						}

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
						if (!cancelled && _stateLinkDragMode == StateLinkDragMode.Link)
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
								StateRef stateRef = new StateRef(draggedOnToState.Asset);
								stateRef._editorPosition = Vector2.zero;
								
								SetStateLink(_draggingState.Asset, _draggingStateLink, stateRef);
							}
							else
							{
								SetStateLink(_draggingState.Asset, _draggingStateLink, new StateRef());
							}
						}

						inputEvent.Use();
						_dragMode = DragType.NotDragging;

						_draggingState = null;
						_draggingStateLink = null;
						_draggingStateLinkIndex = -1;
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

				protected override void OnLoadAsset(StateMachine asset)
				{
#if DEBUG
					_playModeHighlightedState = null;
#endif

					//If no entry state is found create one now
					if (asset._entryState == null)
					{
						asset._entryState = CreateAndAddNewObject<StateMachineEntryState>();
					}

					foreach (StateEditorGUI stateGUI in _editableObjects)
					{
						stateGUI.Asset.SetParent(Asset);
					}

					CenterCamera();
				}
				#endregion

				#region Private Functions
				private void SaveEditorPrefs()
				{
					string prefsXml = Serializer.ToString(_editorPrefs);
					ProjectEditorPrefs.SetString(_editorPrefsTag, prefsXml);
				}

				private void CreateViews()
				{
					_currentZoom = _editorPrefs._zoom;

					if (!string.IsNullOrEmpty(_editorPrefs._fileName))
					{
						LoadFile(_editorPrefs._fileName);
					}
				}

				private void LoadFile(string fileName)
				{
					if (Load(AssetUtils.GetAssetPath(fileName)))
					{
						if (_editorPrefs._fileName != fileName)
						{
							_editorPrefs._fileName = fileName;
							_editorPrefs._stateId = -1;
							SaveEditorPrefs();
						}
					}
					else
					{
						_editorPrefs._fileName = null;
						_editorPrefs._stateId = -1;
						SaveEditorPrefs();
					}
				}

				private bool ShowOnLoadSaveChangesDialog()
				{
					if (HasChanges())
					{
						int option = EditorUtility.DisplayDialogComplex("State Machine Has Been Modified", "Do you want to save the changes you made to the state machine:\n\n" + Asset.name + "\n\nYour changes will be lost if you don't save them.", "Save", "Don't Save", "Cancel");

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
							string titleText = _title + (Asset != null ? " - <b>" + Asset.name : "");

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
								New();
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

				private StateEditorGUI GetStateGUI(State state)
				{
					foreach (StateEditorGUI editorGUI in _editableObjects)
					{
						if (editorGUI.Asset == state)
						{
							return editorGUI;
						}
					}

					return null;
				}
				
				private Vector2 GetLinkStartPosition(StateEditorGUI state, int linkIndex = 0)
				{
					float fraction = 1.0f / ((state.Asset.GetEditorStateLinks()).Length + 1.0f);
					float edgeFraction = fraction * (1 + linkIndex);

					Rect stateRect = GetScreenRect(state.GetBounds());
					return new Vector2(Mathf.Round(stateRect.x + stateRect.width * edgeFraction), Mathf.Round(stateRect.y + stateRect.height - _style._shadowSize - 1.0f));
				}

				private Vector2 GetLinkEndPosition(StateEditorGUI fromState, Vector2 fromPos, StateEditorGUI state, out LinkArrowDirection linkArrowDirection)
				{
					Rect stateRect = GetScreenRect(state.GetBounds());

					if (fromState == state)
					{
						linkArrowDirection = LinkArrowDirection.Left;
						return new Vector2(Mathf.Round(stateRect.x + stateRect.width) + 0.5f, Mathf.Round(stateRect.y + stateRect.height * 0.5f) + 0.5f);
					}
					else
					{
						if (fromPos.y < stateRect.y + stateRect.height * 0.5f)
						{
							linkArrowDirection = LinkArrowDirection.Down;
							return new Vector2(Mathf.Round(stateRect.x + stateRect.width * 0.5f) + 0.5f, Mathf.Round(stateRect.y - _style._linkArrowHeight - 1.0f) + 0.5f);
						}
						else
						{
							if (fromPos.x < stateRect.x + stateRect.width * 0.5f)
							{
								linkArrowDirection = LinkArrowDirection.Right;
								return new Vector2(Mathf.Round(stateRect.x) + 0.5f, Mathf.Round(stateRect.y + stateRect.height * 0.5f) + 0.5f);
							}
							else
							{
								linkArrowDirection = LinkArrowDirection.Left;
								return new Vector2(Mathf.Round(stateRect.x + stateRect.width) + 0.5f, Mathf.Round(stateRect.y + stateRect.height * 0.5f) + 0.5f);
							}
						}
					}
				}

				private void RenderLinkLine(Vector2 startPos, Vector2 endPos, Vector2 labelPos, Color color, bool looped = false, LinkArrowDirection linkArrowDirection = LinkArrowDirection.Down)
				{
					float tangent = kLineTangent * _currentZoom;
					float labelTangentDist;

					if (looped)
					{

						labelTangentDist = tangent * 2f;
					}
					else
					{
						Vector2 line = (endPos - startPos).normalized;
						
						//Get closest point on line to label position
						float lineDot = Vector2.Dot(line, labelPos - startPos);
						Vector2 labelLinePoint = startPos + line * lineDot;

						//Label tangent is based on this distance in line tanget space
						labelTangentDist = Mathf.Abs(Vector2.Dot(labelPos - labelLinePoint, Maths.MathUtils.Cross(line)));
					}

					Vector2 labelTangent;

					if (startPos.y < endPos.y)
					{
						labelTangent = Vector2.up * labelTangentDist * 0.25f;
					}
					else
					{
						labelTangent = Vector2.down * labelTangentDist * 0.25f;
					}

					Vector2 arrowTangent;

					switch (linkArrowDirection)
					{
						case LinkArrowDirection.Left:
							{
								arrowTangent = Vector2.left * tangent;
								break;
							}
						case LinkArrowDirection.Right:
							{
								arrowTangent = Vector2.right * tangent;
								break;
							}
						case LinkArrowDirection.Down:
						default:
							{
								arrowTangent = Vector2.up * tangent;
								break;
							}
					}

					Vector2 startTangent = startPos + Vector2.up * tangent;
					Vector2 endTangent = endPos - arrowTangent;

					Handles.BeginGUI();
					Handles.color = color;

					Handles.DrawBezier(startPos, labelPos, startTangent, labelPos - labelTangent, color, EditorUtils.BezierAATexture, _style._lineLineWidth);
					Handles.DrawBezier(labelPos, endPos, labelPos + labelTangent, endTangent, color, EditorUtils.BezierAATexture, _style._lineLineWidth);

					switch (linkArrowDirection)
					{
						case LinkArrowDirection.Down:
							{
								Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(endPos.x, endPos.y + _style._linkArrowHeight, 0.0f), new Vector3(endPos.x + _style._linkArrowWidth, endPos.y, 0.0f), new Vector3(endPos.x - _style._linkArrowWidth, endPos.y, 0.0f) });
								break;
							}
						case LinkArrowDirection.Left:
							{
								Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(endPos.x, endPos.y, 0.0f), new Vector3(endPos.x + _style._linkArrowHeight, endPos.y + _style._linkArrowWidth, 0.0f), new Vector3(endPos.x + _style._linkArrowHeight, endPos.y - _style._linkArrowWidth, 0.0f) });
								break;
							}
						case LinkArrowDirection.Right:
							{
								Handles.DrawAAConvexPolygon(new Vector3[] { new Vector3(endPos.x, endPos.y, 0.0f), new Vector3(endPos.x - _style._linkArrowHeight, endPos.y + _style._linkArrowWidth, 0.0f), new Vector3(endPos.x - _style._linkArrowHeight, endPos.y - _style._linkArrowWidth, 0.0f) });
								break;
							}
					}


					Handles.EndGUI();
				}

				private void RenderLinkLabel(Vector2 labelPos, string text, float borderSize)
				{
					Vector2 labelSize = _style._linkTextStyle.CalcSize(new GUIContent(text));
					Rect labelRect = new Rect(labelPos.x - (labelSize.x * 0.5f), labelPos.y - (labelSize.y * 0.5f), labelSize.x, labelSize.y);

					//Draw shadow
					Rect shadowRect = new Rect(labelRect.x + _style._shadowSize, labelRect.y + _style._shadowSize, labelRect.width, labelRect.height);
					EditorUtils.DrawColoredRoundedBox(shadowRect, _style._shadowColor, _style._stateCornerRadius);

					if (borderSize > 0f)
					{
						Rect outlineRect = new Rect(labelRect.x - borderSize, labelRect.y - borderSize, labelRect.width + borderSize + borderSize, labelRect.height + borderSize + borderSize);
						EditorUtils.DrawColoredRoundedBox(outlineRect, _style._stateBorderSelectedColor, _style._stateCornerRadius);
					}

					//Draw label background
					EditorUtils.DrawColoredRoundedBox(labelRect, _style._linkDescriptionColor, _style._stateCornerRadius);

					//Draw label
					GUI.Label(labelRect, text, _style._linkTextStyle);
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

				private int GenerateNewNoteId()
				{
					int stateId = 0;

					foreach (StateEditorGUI stateGUI in _editableObjects)
					{
						if (stateGUI.Asset is StateMachineNote)
						{
							stateId++;
						}
					}

					return stateId;
				}

				private int GenerateNewStateId()
				{
					int stateId = 0;

					foreach (StateEditorGUI stateGUI in _editableObjects)
					{
						if (!(stateGUI.Asset is StateMachineEntryState || stateGUI.Asset is StateMachineNote))
						{
							stateId++;
						}
					}

					return stateId;
				}

				private StateEditorGUI FindStateForLink(StateRef link)
				{
					State state = link.State;

					if (state != null)
					{
						foreach (StateEditorGUI stateGUI in _editableObjects)
						{
							if (state == stateGUI.Asset)
							{
								return stateGUI;
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
					return !(editorGUI.Asset is StateMachineEntryState || editorGUI.Asset is StateMachineNote);
				}


				private void SetStateLink(State state, StateMachineEditorLink link, StateRef stateRef)
				{
					Undo.RecordObject(state, "Set link");
					link.SetStateRef(stateRef);
				}

				private Vector2 GetDefaultLinkLabelPos(Vector3 lineStartPos, Vector3 lineStartEnd)
				{
					return new Vector3(lineStartPos.x + ((lineStartEnd.x - lineStartPos.x) * 0.5f), lineStartPos.y + ((lineStartEnd.y - lineStartPos.y) * 0.5f));
				}

#if DEBUG
				private void UpdateInPlayMode()
				{
					_debugging = false;

					if (_editorPrefs._debug)
					{
						StateMachineComponent stateMachine = _editorPrefs._debugObject.GetComponent();
						StateMachineDebug.StateInfo stateInfo = StateMachineDebug.GetStateInfo(stateMachine);

						if (stateInfo != null)
						{
							_debugging = true;

							if (stateInfo._stateMachine != Asset)
							{
								Load(stateInfo._stateMachine);
							}

							if (_playModeHighlightedState != stateInfo._state)
								GetEditorWindow().DoRepaint();

							_playModeHighlightedState = stateInfo._state;

							if (_editorPrefs._debugLockFocus)
								CenterCameraOn(GetStateGUI(_playModeHighlightedState));
						}
					}
				}
#endif
				#endregion
			}
		}
	}
}