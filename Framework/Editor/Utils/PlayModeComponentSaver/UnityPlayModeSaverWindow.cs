#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

using Object = UnityEngine.Object;

namespace Framework
{
	namespace Utils
	{
		namespace Editor
		{
			public class UnityPlayModeSaverWindow : EditorWindow
			{
				#region Constants
				private const float kClearButtonWidth = 24f;
				private const float kClearAllButtonWidth = 220f;
				private const float kSaveModeWidth = 102f;
				private const float kSnapshotButtonWidth = 116f;
				private const float kDefaultNameWidth = 280f;
				private const float kMinNameWidth = 50f;
				private const string kWindowTitle = "Play Mode Saver";				
				private static readonly GUIContent kObjectLabel = new GUIContent("Saved Object"); 
				private static readonly GUIContent kObjectPathLabel = new GUIContent("Object Path");
				private static readonly GUIContent kNoObjectsLabel = new GUIContent("Either right click on any Game Object or Component and click 'Save Play Mode Changes'\nor drag any Game Object or Component into this window.");
				private static readonly GUIContent kObjectsDetailsLabel = new GUIContent("These objects will have their values saved upon leaving Play Mode.\nIf an object has a snapshot saved it will restore to that, otherwise it will keep the values it had upon exiting Play Mode.");
				private static readonly GUIContent kNotInEditModeLabel = new GUIContent("Not in Play Mode.");
				private static readonly string[] kSaveModeOptions = new string[] { "Save on Exit", "Snapshot" };
				private const string kDeletedObj = " <i>(Deleted)</i>";
				private const string kSceneNotLoadedObj = " <i>(Scene not loaded)</i>";
				private const string kSaveSnapshot = " Save Snapshot";
				private const string kClearAllButton = "Clear All Saved Objects";
				private static readonly float kResizerWidth = 6.0f;
				private static readonly float kTextFieldSpace = 6.0f;
				#endregion

				#region Private Data
				private Vector2 _scrollPosition = Vector2.zero;
				private float _nameWidth = kDefaultNameWidth;
				private Rect _nameResizerRect;
				private bool _needsRepaint;
				private enum ResizingState
				{
					NotResizing,
					ResizingName,
				}
				private ResizingState _resizing;
				private int _controlID;
				private float _resizingOffset;
				private GUIStyle _bottomBarInfoStyle;
				private GUIStyle _bottomBarInfoTextStyle;
				private GUIStyle _headerStyle;			
				private GUIStyle _itemNameStyle;
				private GUIStyle _itemButtonStyle;
				private GUIStyle _itemPathStyle;
				private GUIStyle _itemSpaceStyle;
				private GUIStyle _noObjectsStyle;
					
				private GUIContent _clearButtonContent;
				private GUIContent _clearAllButtonContent;
				private GUIContent _saveSnapshotContent;
				private Texture _scriptIcon;
				#endregion

				#region Internal Interface
				internal static UnityPlayModeSaverWindow Open(bool focus)
				{
					UnityPlayModeSaverWindow window = (UnityPlayModeSaverWindow)GetWindow(typeof(UnityPlayModeSaverWindow), false, kWindowTitle, focus);

					if (!focus)
						window.Repaint();

					return window;
				}
				#endregion

				#region Unity Messages
				private void OnGUI()
				{
					Initialize();

					_needsRepaint = false;

					if (UnityPlayModeSaver.CurrentPlayModeState == PlayModeStateChange.EnteredPlayMode)
					{
						EditorGUILayout.BeginVertical();
						{
							DrawTitleBar();
							DrawTable();
							DrawBottomButton();
						}
						EditorGUILayout.EndVertical();

						HandleInput();
					}
					else
					{
						EditorGUILayout.LabelField(kNotInEditModeLabel, _noObjectsStyle, GUILayout.ExpandHeight(true));
					}
						
					if (_needsRepaint)
						Repaint();
				}
				#endregion

				#region Private Functions
				private void Initialize()
				{
					if (_headerStyle == null)
					{
						_headerStyle = new GUIStyle(EditorStyles.toolbarButton)
						{
							alignment = TextAnchor.MiddleLeft,
							padding = new RectOffset(8, 8, 0, 0),
							fontStyle = FontStyle.Italic,
						};
					}

					if (_itemNameStyle == null)
					{
						_itemNameStyle = new GUIStyle(EditorStyles.toolbarTextField)
						{
							alignment = TextAnchor.MiddleLeft,
							margin = new RectOffset(0, 0, 2, 3),
							padding = new RectOffset(8, 8, 0, 0),
							fixedHeight = 0,
							stretchHeight = true,
							stretchWidth = true,
							richText = true,
						};
					}

					if (_itemButtonStyle == null)
					{
						_itemButtonStyle = new GUIStyle(EditorStyles.toolbarButton)
						{
							alignment = TextAnchor.MiddleLeft,
						};
					}

					if (_itemPathStyle == null)
					{
						_itemPathStyle = new GUIStyle(EditorStyles.toolbarSearchField)
						{
							fontSize = 12,
							margin = new RectOffset(0, 0, 2, 3),
							padding = new RectOffset(16, 8, 0, 0),
							fixedHeight = 0,
							stretchHeight = true,
							stretchWidth = true,
						};
					}

					if (_itemSpaceStyle == null)
					{
						_itemSpaceStyle = new GUIStyle(EditorStyles.label)
						{
							margin = new RectOffset(0, 0, 0, 0),
							padding = new RectOffset(0, 0, 0, 0),
						};
					}

					if (_noObjectsStyle == null)
					{
						_noObjectsStyle = new GUIStyle(EditorStyles.label)
						{
							alignment = TextAnchor.MiddleCenter,
							stretchWidth = true,
							stretchHeight = true
						};
					}

					if (_bottomBarInfoStyle == null)
					{
						_bottomBarInfoStyle = new GUIStyle(EditorStyles.toolbar)
						{
							fixedHeight = 42,
						};
					}

					if (_bottomBarInfoTextStyle == null)
					{
						_bottomBarInfoTextStyle = new GUIStyle(EditorStyles.toolbarButton)
						{
							alignment = TextAnchor.MiddleCenter,
							fixedHeight = 40,
							fontStyle = FontStyle.Italic
						};
					}

					if (_saveSnapshotContent == null)
					{
						titleContent = new GUIContent(kWindowTitle, EditorGUIUtility.IconContent("SaveAs").image);
						minSize = new Vector2(kDefaultNameWidth, kDefaultNameWidth);

						_saveSnapshotContent = new GUIContent(kSaveSnapshot, EditorGUIUtility.IconContent("SaveAs").image);
					}

					if (_clearButtonContent == null)
					{
						_clearButtonContent = EditorGUIUtility.IconContent("d_winbtn_win_close");
					}

					if (_clearAllButtonContent == null)
					{
						_clearAllButtonContent = new GUIContent(kClearAllButton);
					}

					if (_scriptIcon == null)
					{
						_scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image;
					}
				}

				private void DrawTitleBar()
				{
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					{
						//Clear Button
						EditorGUILayout.Space(kClearButtonWidth, false);

						//Object name
						GUILayout.Label(kObjectLabel, _headerStyle, GUILayout.Width(_nameWidth - _scrollPosition.x));

						//Name Resizer
						RenderResizer(ref _nameResizerRect);

						//Save Mode
						GUILayout.Label("Save Mode", _headerStyle, GUILayout.Width(kSaveModeWidth));

						//Snapshot buttons
						GUILayout.Label(GUIContent.none, _headerStyle, GUILayout.Width(kSnapshotButtonWidth));

						//Object Path
						GUILayout.Label(kObjectPathLabel, _headerStyle);
					}
					EditorGUILayout.EndHorizontal();
				}

				private void DrawTable()
				{
					bool origGUIenabled = GUI.enabled;

					_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
					{
						for (int i=0; i< UnityPlayModeSaver.SavedObjects.Count;)
						{
							bool itemRemoved = false;

							UnityPlayModeSaver.SavedObject savedObject = UnityPlayModeSaver.SavedObjects[i];

							EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
							{
								//Clear object button
								if (GUILayout.Button(_clearButtonContent, EditorStyles.toolbarButton, GUILayout.Width(kClearButtonWidth)))
								{
									itemRemoved = true;
								}

								//Spacer
								GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kTextFieldSpace));

								// Object button
								{
									string name = " " + GetObjectName(savedObject);
										
									if (savedObject._object == null)
									{
										GUI.enabled = false;

										Scene scene = SceneManager.GetSceneByPath(savedObject._scenePath);

										if (!scene.IsValid() || !scene.isLoaded)
										{
											name += kSceneNotLoadedObj;
										}
										else
										{
											name += kDeletedObj;
										}
									}

									Texture icon = EditorGUIUtility.ObjectContent(null, savedObject._type).image;

									if (icon == null)
									{
										icon = _scriptIcon;
									}

									GUIContent buttonContent = new GUIContent(name, icon);

									if (GUILayout.Button(buttonContent, _itemNameStyle, GUILayout.Width(_nameWidth - kResizerWidth)))
									{
										FocusOnObject(savedObject._object);
									}

									GUI.enabled = origGUIenabled;
								}

								//Spacer
								GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kResizerWidth));

								//Save Mode
								{
									int selected = savedObject._hasSnapshot ? 1 : 0;
									int newSelected = EditorGUILayout.Popup(selected, kSaveModeOptions, EditorStyles.toolbarDropDown, GUILayout.Width(kSaveModeWidth));

									if (selected != newSelected)
									{
										if (newSelected == 1)
										{
											UnityPlayModeSaver.SaveSnapshot(i);
										}
										else
										{
											UnityPlayModeSaver.ClearSnapshot(i);

											//Check objects scene is still loaded, if not remove object
											Scene scene = SceneManager.GetSceneByPath(savedObject._scenePath);

											if (!scene.IsValid() || !scene.isLoaded)
											{
												itemRemoved = true;
											}
										}
									}
								}

								//Snap shot button
								{
									if (savedObject._object == null)
									{
										GUI.enabled = false;
									}

									if (GUILayout.Button(_saveSnapshotContent, _itemButtonStyle, GUILayout.Width(kSnapshotButtonWidth)))
									{
										UnityPlayModeSaver.SaveSnapshot(i);
									}

									GUI.enabled = origGUIenabled;
								}

								//Spacer
								GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kTextFieldSpace));

								//Object path
								if (GUILayout.Button(savedObject._path + GetObjectName(savedObject), _itemPathStyle, GUILayout.ExpandWidth(true)))
								{
									FocusOnObject(savedObject._object);
								}

								GUILayout.Label(GUIContent.none, _itemSpaceStyle, GUILayout.Width(kTextFieldSpace));
							}
							EditorGUILayout.EndHorizontal();

							if (itemRemoved)
							{
								UnityPlayModeSaver.ClearSavedObject(i);
								_needsRepaint = true;
							}
							else
							{
								i++;
							}
						}

						if (UnityPlayModeSaver.SavedObjects.Count == 0)
						{
							EditorGUILayout.LabelField(kNoObjectsLabel, _noObjectsStyle, GUILayout.ExpandHeight(true));
						}
					}
					EditorGUILayout.EndScrollView();
				}

				private void DrawBottomButton()
				{
					EditorGUILayout.BeginHorizontal(_bottomBarInfoStyle);
					{
						GUILayout.Label(kObjectsDetailsLabel, _bottomBarInfoTextStyle);
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					{
						GUILayout.FlexibleSpace();

						if (GUILayout.Button(_clearAllButtonContent, GUILayout.Width(kClearAllButtonWidth)))
						{
							UnityPlayModeSaver.ClearCache();
							_needsRepaint = true;
						}

						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();
				}

				private void HandleInput()
				{
					Event inputEvent = Event.current;

					if (inputEvent == null)
						return;

					EventType controlEventType = inputEvent.GetTypeForControl(_controlID);

					if (_resizing != ResizingState.NotResizing && inputEvent.rawType == EventType.MouseUp)
					{
						_resizing = ResizingState.NotResizing;
						_needsRepaint = true;
					}

					switch (controlEventType)
					{
						case EventType.MouseDown:
							{
								if (inputEvent.button == 0)
								{
									if (_nameResizerRect.Contains(inputEvent.mousePosition))
									{
										_resizing = ResizingState.ResizingName;
									}
									else
									{
										_resizing = ResizingState.NotResizing;
									}

									if (_resizing != ResizingState.NotResizing)
									{
										inputEvent.Use();
										_resizingOffset = inputEvent.mousePosition.x;
									}
								}
							}
							break;

						case EventType.MouseUp:
							{
								if (_resizing != ResizingState.NotResizing)
								{
									inputEvent.Use();
									_resizing = ResizingState.NotResizing;
								}
							}
							break;

						case EventType.MouseDrag:
							{
								if (_resizing != ResizingState.NotResizing)
								{
									if (_resizing == ResizingState.ResizingName)
									{
										_nameWidth += (inputEvent.mousePosition.x - _resizingOffset);
										_nameWidth = Math.Max(_nameWidth, kMinNameWidth);
									}

									_resizingOffset = inputEvent.mousePosition.x;
									_needsRepaint = true;
								}
							}
							break;
						case EventType.DragUpdated:
							{
								bool objectsAreAllowed = true;

								foreach (Object obj in DragAndDrop.objectReferences)
								{
									if (!(obj is GameObject) && !(obj is Component))
									{
										objectsAreAllowed = false;
										break;
									}
								}

								DragAndDrop.visualMode = objectsAreAllowed ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
							}
							break;

						case EventType.DragPerform:
							{
								foreach (Object obj in DragAndDrop.objectReferences)
								{
									if (!UnityPlayModeSaver.IsObjectRegistered(obj, out _))
									{
										if (obj is GameObject gameObject)
										{
											UnityPlayModeSaver.RegisterSavedObject(gameObject);
										}
										else if (obj is Component component)
										{
											UnityPlayModeSaver.RegisterSavedObject(component);
										}
									}
								}

								DragAndDrop.AcceptDrag();
							}
							break;
					}
				}

				private void RenderResizer(ref Rect rect)
				{
					GUILayout.Box(GUIContent.none, EditorStyles.toolbar, GUILayout.Width(kResizerWidth), GUILayout.ExpandHeight(true));
					rect = GUILayoutUtility.GetLastRect();
					EditorGUIUtility.AddCursorRect(rect, MouseCursor.SplitResizeLeftRight);
				}

				private static void FocusOnObject(Object obj)
				{
					Selection.activeObject = obj;
					SceneView.FrameLastActiveSceneView();
					EditorGUIUtility.PingObject(obj);
				}

				private string GetObjectName(UnityPlayModeSaver.SavedObject savedObject)
				{
					if (savedObject._object == null)
					{
						return savedObject._name;
					}
					else
					{
						return UnityPlayModeSaver.GetObjectName(savedObject._object);
					}
				}
				#endregion
			}
		}
	}
}

#endif