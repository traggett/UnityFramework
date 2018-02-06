using UnityEditor;
using UnityEngine;
using System;

namespace Framework
{
	using Utils;
	using Utils.Editor;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			public sealed class LocalisationEditorWindow : EditorWindow
			{
				private static readonly string kWindowWindowName = "Localisation";
				private static readonly string kWindowTitle = "Localisation Editor";
				private static readonly string kWindowTag = "Localisation";
				private static readonly string kKeySizePref = "KeySize";

				private static readonly float kMinKeysWidth = 180.0f;

				private static readonly float kToolBarHeight = 60.0f;

				private static readonly Color kSelectedTextLineBackgroundColor = new Color(1.0f, 0.8f, 0.1f, 1.0f);
				private static readonly Color kTextLineBackgroundColorA = new Color(0.7f, 0.7f, 0.7f, 1.0f);
				private static readonly Color kTextLineBackgroundColorB = new Color(0.82f, 0.82f, 0.82f, 1.0f);
				private static readonly Color kKeyBackgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
				private static readonly Color kTextBackgroundColorA = new Color(0.9f, 0.9f, 0.9f, 1.0f);
				private static readonly Color kTextBackgroundColorB = new Color(0.98f, 0.98f, 0.98f, 1.0f); 

				private Rect _resizerRect;
				private bool _resizing;
				private int _controlID;
				private float _resizingOffset;
				private float _keyWidth = 200.0f;
				private string _selectedKey;
				private Vector2 _scrollPosition;
				private bool _needsRepaint;

				private GUIStyle _titleStyle;
				private GUIStyle _keyStyle;
				private GUIStyle _textStyle;
				
				//Make localisted string refs readonly?
				//add edit button next to text
				//that button opens the localisation window, with the key selected and scrolled to it
				//Still allow add key button - can add new keys from refs as before
				
				
				//add new key button to localisation window
				//add delete key button to localisation window


				#region Menu Stuff
				private static LocalisationEditorWindow _instance = null;

				[MenuItem("Localisation/Localisation Strings")]
				private static void CreateWindow()
				{
					// Get existing open window or if none, make a new one:
					_instance = (LocalisationEditorWindow)GetWindow(typeof(LocalisationEditorWindow), false, kWindowWindowName);
					_instance.Init();
				}
				
				[MenuItem("Localisation/Reload Strings")]
				private static void MenuReloadStrings()
				{
					Localisation.LoadStrings();
				}

				[MenuItem("Localisation/Save Strings")]
				private static void MenuSaveStrings()
				{
					Localisation.SaveStrings();
				}
				#endregion		

				#region EditorWindow
				void Update()
				{
					
				}

				void OnGUI()
				{
					InitGUIStyles();

					_needsRepaint = false;

					EditorGUILayout.BeginVertical();
					{
						//Render tool bar
						RenderTitleBar();

						//Render keys / text
						RenderTable();

						//Render edit buttons
						RenderEditButtons();
					}
					EditorGUILayout.EndVertical();

					HandleInput();

					if (_needsRepaint)
						Repaint();
				}

				void OnDestroy()
				{
					
				}
				#endregion

				private void Init()
				{
					_controlID = GUIUtility.GetControlID(FocusType.Passive);
					_keyWidth = EditorPrefs.GetFloat(kWindowTag+"."+ kKeySizePref, kMinKeysWidth);
				}

				private void InitGUIStyles()
				{
					//if (_titleStyle == null)
					{
						_titleStyle = new GUIStyle(EditorStyles.label);
						_titleStyle.richText = true;
						_titleStyle.alignment = TextAnchor.MiddleCenter;
					}

					//if (_keyStyle == null)
					{
						_keyStyle = new GUIStyle(EditorStyles.helpBox);
						_keyStyle.margin = new RectOffset(0, 0, 0, 0);
						//_keyStyle.fontStyle = FontStyle.Bold;
						//_keyStyle.fontSize = 11;
					}

					//if (_textStyle == null)
					{
						_textStyle = new GUIStyle(EditorStyles.textArea);
						_textStyle.margin = new RectOffset(1, 1, 1, 1);
					}
				}

				private void RenderTitleBar()
				{
					EditorGUILayout.BeginVertical(GUILayout.Height(kToolBarHeight));
					{
						//Title
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							string titleText = kWindowTitle + " - <b>Localisation.xml</b>";

							if (Localisation.HasUnsavedChanges())
								titleText += "<b>*</b>";

							EditorGUILayout.LabelField(titleText, _titleStyle);
						}
						EditorGUILayout.EndHorizontal();

						//Load save
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (GUILayout.Button("Save", EditorStyles.toolbarButton))
							{
								Localisation.SaveStrings();
							}

							if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
							{
								Localisation.LoadStrings();
							}
							
							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						//Headers
						EditorGUILayout.BeginHorizontal();
						{
							//Key
							EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(_keyWidth));
							{
								if (GUILayout.Button("Key", EditorStyles.toolbarButton))
								{
									//Change sort method
								}
							}
							EditorGUILayout.EndHorizontal();

							//Resizer
							RenderResizer();

							//Text
							EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
							{
								EditorGUI.BeginChangeCheck();
								SystemLanguage language = (SystemLanguage)EditorGUILayout.EnumPopup(Localisation.GetCurrentLanguage(), EditorStyles.toolbarButton);
								if (EditorGUI.EndChangeCheck())
								{
									Localisation.SetLanguage(language);
								}

								GUILayout.FlexibleSpace();
							}
							EditorGUILayout.EndHorizontal();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}

				private void RenderResizer()
				{
					GUILayout.Box(string.Empty, EditorStyles.toolbar, GUILayout.Width(1.0f), GUILayout.ExpandHeight(true));
					
					_resizerRect = GUILayoutUtility.GetLastRect();
					_resizerRect.x -= 8;
					_resizerRect.width += 16;
					EditorGUIUtility.AddCursorRect(_resizerRect, MouseCursor.SplitResizeLeftRight);
				}
				
				private void RenderTable()
				{
					_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
					{
						EditorGUILayout.BeginVertical();
						{
							string[] keys = Localisation.GetStringKeys();

							for (int i = 1; i < keys.Length; i++)
							{
								string text = Localisation.GetUnformattedString(keys[i]);
								int numLines = StringUtils.GetNumberOfLines(text);
								float height = (EditorGUIUtility.singleLineHeight - 2.0f) * numLines + 4.0f;

								Color origBackgroundColor = GUI.backgroundColor;
								GUI.backgroundColor = i % 2 == 0 ? kTextLineBackgroundColorA : kTextLineBackgroundColorB;

								if (keys[i] == _selectedKey)
									GUI.backgroundColor = kSelectedTextLineBackgroundColor;

								EditorGUILayout.BeginHorizontal(EditorUtils.ColoredRoundedBoxStyle, GUILayout.Height(height));
								{
									GUI.backgroundColor = kKeyBackgroundColor;
									if (GUILayout.Button(keys[i], _keyStyle, GUILayout.Width(_keyWidth), GUILayout.ExpandHeight(true)))
									{
										_selectedKey = keys[i];
										EditorGUI.FocusTextInControl(string.Empty);
									}							

									GUI.backgroundColor = i % 2 == 0 ? kTextBackgroundColorA : kTextBackgroundColorB;
									EditorGUI.BeginChangeCheck();
									text = EditorGUILayout.TextArea(text, _textStyle, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
									if (EditorGUI.EndChangeCheck())
									{
										Localisation.UpdateString(keys[i], Localisation.GetCurrentLanguage(), text);
									}
								}
								EditorGUILayout.EndHorizontal();
								GUI.backgroundColor = origBackgroundColor;
							}
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndScrollView();
				}

				private void RenderEditButtons()
				{
					//Load save
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					{
						if (GUILayout.Button("New", EditorStyles.toolbarButton))
						{
							
						}

						if (GUILayout.Button("Delete", EditorStyles.toolbarButton))
						{
							DeleteSelected();
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

					if (_resizing && inputEvent.rawType == EventType.MouseUp)
					{
						_resizing = false;
						_needsRepaint = true;
					}

					switch (controlEventType)
					{
						case EventType.MouseDown:
							{
								if (inputEvent.button == 0 && _resizerRect.Contains(inputEvent.mousePosition))
								{
									inputEvent.Use();

									_resizing = true;
									_resizingOffset = inputEvent.mousePosition.x;
								}
							}
							break;

						case EventType.MouseUp:
							{
								if (_resizing)
								{
									inputEvent.Use();
									_resizing = false;
								}
							}
							break;

						case EventType.MouseDrag:
							{
								if (_resizing)
								{
									_keyWidth += (inputEvent.mousePosition.x - _resizingOffset);
									_keyWidth = Math.Max(_keyWidth, kMinKeysWidth);
									EditorPrefs.SetFloat(kWindowTag + "." + kKeySizePref, _keyWidth);

									_resizingOffset = inputEvent.mousePosition.x;

									_needsRepaint = true;
								}
							}
							break;
					
						case EventType.ValidateCommand:
							{
								if (inputEvent.commandName == "SoftDelete")
								{
									DeleteSelected();
								}
								else if (inputEvent.commandName == "UndoRedoPerformed")
								{
									_needsRepaint = true;
								}
							}
							break;
					}
				}

				private void DeleteSelected()
				{
					if (!string.IsNullOrEmpty(_selectedKey))
					{
						Localisation.DeleteString(_selectedKey);
						_selectedKey = null;
						_needsRepaint = true;
					}
				}
			}
		}
	}
}