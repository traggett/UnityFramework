using UnityEditor;
using UnityEngine;
using System;

namespace Framework
{
	using Serialization;
	using Utils;
	using Utils.Editor;

	namespace LocalisationSystem
	{
		namespace Editor
		{
			public sealed class LocalisationEditorWindow : EditorWindow
			{
				public static readonly int kDefaultFontSize = 12;
				public static readonly float kDefaultKeysWidth = 380.0f;

				private static readonly string kWindowWindowName = "Localisation";
				private static readonly string kWindowTitle = "Localisation Editor";
				private static readonly string kEditorPrefKey = "LocalisationEditor.Settings";
				private static readonly string kKeySizePref = "KeySize";
				private static readonly string kFontSizePref = "FontSize";

				private static readonly float kMinKeysWidth = 180.0f;
				private static readonly float kToolBarHeight = 60.0f;
				private static readonly float kBottomBarHeight = 16.0f;

				private static readonly Color kSelectedTextLineBackgroundColor = new Color(1.0f, 0.8f, 0.1f, 1.0f);
				private static readonly Color kSelectedButtonsBackgroundColor = new Color(1.0f, 0.8f, 0.1f, 0.75f);
				private static readonly Color kTextLineBackgroundColorA = new Color(0.7f, 0.7f, 0.7f, 1.0f);
				private static readonly Color kTextLineBackgroundColorB = new Color(0.82f, 0.82f, 0.82f, 1.0f);
				private static readonly Color kKeyBackgroundColor = new Color(0.9f, 0.9f, 0.9f, 1.0f);
				private static readonly Color kTextBackgroundColorA = new Color(0.9f, 0.9f, 0.9f, 1.0f);
				private static readonly Color kTextBackgroundColorB = new Color(0.98f, 0.98f, 0.98f, 1.0f);

				private static LocalisationEditorWindow _instance = null;

				private LocalisationEditorPrefs _editorPrefs;
				private Rect _resizerRect;
				private bool _resizing;
				private int _controlID;
				private float _resizingOffset;
				private Vector2 _scrollPosition;
				private bool _needsRepaint;
				private string _addNewKey = string.Empty;
				private bool _editingKeyName;

				private GUIStyle _titleStyle;
				private GUIStyle _keyStyle;
				private GUIStyle _textStyle;

				private enum eKeySortOrder
				{
					None,
					Asc,
					Desc
				}
				private eKeySortOrder _sortOrder;

				//Havve key for glypths add button for each char in it at bottom which inserts them into local


				#region Menu Stuff
				[MenuItem("Window/Localisation Editor")]
				private static void CreateWindow()
				{
					// Get existing open window or if none, make a new one:
					_instance = (LocalisationEditorWindow)GetWindow(typeof(LocalisationEditorWindow), false, kWindowWindowName);
					_instance.Init();
				}
				#endregion		

				#region EditorWindow
				void OnGUI()
				{
					CreateEditor();

					_needsRepaint = false;


					EditorGUILayout.BeginVertical();
					{
						RenderTitleBar();
						RenderTable();
						RenderBottomBar();
					}
					EditorGUILayout.EndVertical();

					HandleInput();

					if (_needsRepaint)
						Repaint();
				}

				void OnDestroy()
				{
					if (Localisation.HasUnsavedChanges())
					{
						if (EditorUtility.DisplayDialog("Localisation Table Has Been Modified", "Do you want to save the changes you made to the table?\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
						{
							Localisation.SaveStrings();
						}
						else
						{
							Localisation.LoadStrings();
						}
					}
				}
				#endregion

				public static void EditString(string key)
				{
					if (_instance == null)
						CreateWindow();
					
					_instance.SelectKey(key);
				}

				private void CreateEditor()
				{
					if (_instance == null || _instance._editorPrefs == null)
					{
						_instance = (LocalisationEditorWindow)GetWindow(typeof(LocalisationEditorWindow), false, kWindowWindowName);
						_instance.Init();
					}

					InitGUIStyles();
				}

				private void Init()
				{
					string editorPrefsText = ProjectEditorPrefs.GetString(kEditorPrefKey, "");
					try
					{
						_editorPrefs = Serializer.FromString<LocalisationEditorPrefs>(editorPrefsText);
					}
					catch
					{
						_editorPrefs = null;
					}

					if (_editorPrefs == null)
					{
						_editorPrefs = new LocalisationEditorPrefs();
					}

					_controlID = GUIUtility.GetControlID(FocusType.Passive);
				}

				private void InitGUIStyles()
				{
					if (_titleStyle == null)
					{
						_titleStyle = new GUIStyle(EditorStyles.label);
						_titleStyle.richText = true;
						_titleStyle.alignment = TextAnchor.MiddleCenter;
					}

					if (_keyStyle == null)
					{
						_keyStyle = new GUIStyle(EditorStyles.helpBox);
						_keyStyle.margin = new RectOffset(0, 0, 0, 0);
						_keyStyle.fontSize = _editorPrefs._fontSize;
						_keyStyle.padding.left = 8;
					}

					if (_textStyle == null)
					{
						_textStyle = new GUIStyle(EditorStyles.textArea);
						_textStyle.margin = new RectOffset(1, 1, 1, 1);
						_textStyle.font = _keyStyle.font;
						_textStyle.fontSize = _editorPrefs._fontSize;
						_textStyle.padding = new RectOffset(4, 4, 4, 4);

						Font font = _editorPrefs._font.GetAsset();
						if (font == null)
							_textStyle.font = _keyStyle.font;
						else
							_textStyle.font = font;
					}
				}

				private void SaveEditorPrefs()
				{
					string prefsXml = Serializer.ToString(_editorPrefs);
					ProjectEditorPrefs.SetString(kEditorPrefKey, prefsXml);
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

							EditorGUILayout.Separator();

							GUILayout.Button("Scale", EditorStyles.toolbarButton);

							int fontSize = EditorGUILayout.IntSlider(_editorPrefs._fontSize, 8, 16);

							if (GUILayout.Button("Reset Scale", EditorStyles.toolbarButton))
							{
								fontSize = kDefaultFontSize;
							}

							if (_editorPrefs._fontSize != fontSize)
							{
								_editorPrefs._fontSize = fontSize;
								_keyStyle.fontSize = _editorPrefs._fontSize;
								_textStyle.fontSize = _editorPrefs._fontSize;
								SaveEditorPrefs();
							}

							EditorGUILayout.Separator();

							GUILayout.Button("Font", EditorStyles.toolbarButton);

							Font currentFont = _editorPrefs._font.GetAsset();
							Font font = (Font)EditorGUILayout.ObjectField(currentFont, typeof(Font), false);

							if (currentFont != font)
							{
								_editorPrefs._font = new EditorAssetRef<Font>(font);

								if (font == null)
									_textStyle.font = _keyStyle.font;
								else
									_textStyle.font = font;

								SaveEditorPrefs();
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						//Headers
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							//Key
							if (GUILayout.Button("Key", EditorStyles.toolbarButton, GUILayout.Width(_editorPrefs._keyWidth - 11)))
							{
								_sortOrder = _sortOrder == eKeySortOrder.Desc ? eKeySortOrder.Asc : eKeySortOrder.Desc;
							}

							EditorGUILayout.Separator();

							//Resizer
							RenderResizer();

							//EditorGUILayout.Separator();

							//Text
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
					EditorGUILayout.EndVertical();
				}

				private void RenderResizer()
				{
					GUILayout.Box(string.Empty, EditorStyles.toolbar, GUILayout.Width(4.0f), GUILayout.ExpandHeight(true));
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
							string[] keys = GetKeys();

							for (int i = 1; i < keys.Length; i++)
							{
								bool selected = keys[i] == _editorPrefs._selectedKey;
								string text = Localisation.GetUnformattedString(keys[i]);
								int numLines = StringUtils.GetNumberOfLines(text);
								
								Color origBackgroundColor = GUI.backgroundColor;
								GUI.backgroundColor = selected ? kSelectedTextLineBackgroundColor : i % 2 == 0 ? kTextLineBackgroundColorA : kTextLineBackgroundColorB;

								EditorGUILayout.BeginHorizontal(EditorUtils.ColoredRoundedBoxStyle);
								{
									GUI.backgroundColor = kKeyBackgroundColor;

									if (selected && _editingKeyName)
									{
										EditorGUI.BeginChangeCheck();
										string key = EditorGUILayout.DelayedTextField(keys[i], _textStyle, GUILayout.Width(_editorPrefs._keyWidth), GUILayout.ExpandHeight(true));
										if (EditorGUI.EndChangeCheck())
										{
											_editingKeyName = false;
											Localisation.ChangeKey(keys[i], key);
											_needsRepaint = true;
										}
									}
									else
									{
										if (GUILayout.Button(keys[i], _keyStyle, GUILayout.Width(_editorPrefs._keyWidth), GUILayout.ExpandHeight(true)))
										{
											_editorPrefs._selectedKey = keys[i];
											_editingKeyName = false;
											EditorGUI.FocusTextInControl(string.Empty);
										}
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

								if (selected)
								{
									GUI.backgroundColor = kSelectedButtonsBackgroundColor;
									EditorGUILayout.BeginHorizontal(EditorUtils.ColoredRoundedBoxStyle);
									{
										GUI.backgroundColor = origBackgroundColor;

										if (GUILayout.Button("Edit Key", EditorStyles.toolbarButton))
										{
											_editingKeyName = true;
										}

										if (GUILayout.Button("Delete", EditorStyles.toolbarButton))
										{
											DeleteSelected();
										}

										GUILayout.FlexibleSpace();
									}
									EditorGUILayout.EndHorizontal();
								}

								GUI.backgroundColor = origBackgroundColor;
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndScrollView();
				}

				private void RenderBottomBar()
				{
					EditorGUILayout.Separator();

					EditorGUILayout.BeginHorizontal(GUILayout.Height(kBottomBarHeight));
					{
						if (GUILayout.Button("Add New", EditorStyles.toolbarButton, GUILayout.Width(_editorPrefs._keyWidth)))
						{
							if (!Localisation.IsKeyInTable(_addNewKey) && !string.IsNullOrEmpty(_addNewKey))
							{
								Localisation.UpdateString(_addNewKey, Localisation.GetCurrentLanguage(), string.Empty);
							}
						}
						
						string[] folders = Localisation.GetStringFolders();
						int currentFolderIndex = 0;
						string currentFolder;
						for (int i = 0; i < folders.Length; i++)
						{
							//To do - if the first bit of our folder exists then thats the current folder eg Roles/NewRole/Name - Roles/
							if (folders[i] == Localisation.GetFolderName(_addNewKey))
							{
								currentFolderIndex = i;
								break;
							} 
						}
						
						EditorGUI.BeginChangeCheck();
						int newFolderIndex = EditorGUILayout.Popup(currentFolderIndex, folders);
						currentFolder = newFolderIndex == 0 ? "" : folders[newFolderIndex];
						if (EditorGUI.EndChangeCheck())
						{
							if (newFolderIndex != 0)
								_addNewKey = currentFolder + "/" + Localisation.GetKeyWithoutFolder(_addNewKey);
							else if (currentFolderIndex != 0)
								_addNewKey = Localisation.GetKeyWithoutFolder(_addNewKey);
						}
						
						EditorGUILayout.LabelField("/", GUILayout.Width(8));

						if (newFolderIndex != 0)
						{
							string newAddKey = EditorGUILayout.TextField(Localisation.GetKeyWithoutFolder(_addNewKey));
							_addNewKey = currentFolder + "/" + newAddKey;
						}
						else
						{
							string newAddKey = EditorGUILayout.TextField(_addNewKey);
							_addNewKey = newAddKey;
						}

						GUILayout.FlexibleSpace();
					}
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Separator();
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
									_editorPrefs._keyWidth += (inputEvent.mousePosition.x - _resizingOffset);
									_editorPrefs._keyWidth = Math.Max(_editorPrefs._keyWidth, kMinKeysWidth);
									SaveEditorPrefs();

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
					if (!string.IsNullOrEmpty(_editorPrefs._selectedKey))
					{
						Localisation.DeleteString(_editorPrefs._selectedKey);
						_editorPrefs._selectedKey = null;
						SaveEditorPrefs();
						_needsRepaint = true;
					}
				}

				private void SelectKey(string key)
				{
					Focus();

					_editorPrefs._selectedKey = key;
					EditorGUI.FocusTextInControl(string.Empty);
					SaveEditorPrefs();

					_needsRepaint = true;

					string[] keys = GetKeys();

					float toSelected = 0.0f;

					for (int i = 1; i < keys.Length; i++)
					{
						if (keys[i] == _editorPrefs._selectedKey)
							break;

						string text = Localisation.GetUnformattedString(keys[i]);

						Vector2 textSize = _keyStyle.CalcSize(new GUIContent(text));
						Vector2 keySize = _textStyle.CalcSize(new GUIContent(keys[i]));

						toSelected += Mathf.Max(keySize.y, textSize.y);
					}

					float scrollAreaHeight = this.position.height - kToolBarHeight - kBottomBarHeight;
					_scrollPosition.y = Mathf.Max(toSelected - scrollAreaHeight * 0.4f, 0.0f);
				}

				private string[] GetKeys()
				{
					string[] keys = Localisation.GetStringKeys();

					switch (_sortOrder)
					{
						case eKeySortOrder.Asc:
							Array.Sort(keys, StringComparer.InvariantCulture);
							break;
						case eKeySortOrder.Desc:
							Array.Sort(keys, StringComparer.InvariantCulture);
							Array.Reverse(keys);
							break;
					}

					return keys;
				}
			}
		}
	}
}