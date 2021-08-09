using UnityEditor;
using UnityEngine;

using System;
using System.Collections.Generic;

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
				public static readonly int kMinFontSize = 8;
				public static readonly int kMaxFontSize = 30;
				public static readonly float kDefaultKeysWidth = 380.0f;
				public static readonly float kDefaultFirstLangagueWidth = 580.0f;

				private static readonly string kWindowWindowName = "Localisation";
				private static readonly string kEditorPrefKey = "LocalisationEditor.Settings";
				private static readonly float kMinKeysWidth = 240.0f;
				private static readonly float kResizerWidth = 8.0f;

				private static readonly Color kTextLineBackgroundColorA = new Color(0.7f, 0.7f, 0.7f, 1.0f);
				private static readonly Color kTextLineBackgroundColorB = new Color(0.82f, 0.82f, 0.82f, 1.0f);
				private static readonly Color kSelectedTextLineBackgroundColor = new Color(1f, 1f, 1f, 1f);
				private static readonly Color kTextBackgroundColorA = new Color(0.88f, 0.88f, 0.88f, 1.0f);
				private static readonly Color kTextBackgroundColorB = new Color(0.98f, 0.98f, 0.98f, 1.0f);
				private static readonly Color kSelectedTextColor = new Color(129f / 255f, 180f / 255f, 255f / 255f, 1.0f);

				private static LocalisationEditorWindow _instance = null;

				private LocalisationEditorPrefs _editorPrefs;
				private Rect _keysResizerRect;
				private Rect _languageResizerRect;

				private enum ResizingState
				{
					NotResizing,
					ResizingKeys,
					ResizingLangauge,
				}

				private ResizingState _resizing;
				private int _controlID;
				private float _resizingOffset;
				private Vector2 _scrollPosition;
				private bool _needsRepaint;
				private string _addNewKey = string.Empty;
				private string _editingKeyName;
				private string _filter;

				private GUIStyle _tableStyle;
				private GUIStyle _keyStyle;
				private GUIStyle _selectedKeyStyle;
				private GUIStyle _editKeyStyle;
				private GUIStyle _textStyle;
				private GUIStyle _selectedTextStyle;

				private int _viewStartIndex;
				private int _viewEndIndex;
				private float _contentHeight;
				private float _contentStart;

				private bool _keysDirty;


				private double lastClickTime = 0d;
				private int _lastClickKeyIndex;

				private string[] _keys;

				private enum eKeySortOrder
				{
					Asc,
					Desc
				}
				private eKeySortOrder _sortOrder;

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
					InitGUIStyles();

					_needsRepaint = false;
					_keysDirty = false;

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
							Localisation.ReloadStrings();
						}
					}
				}
				#endregion

				public static void EditString(string key)
				{
					if (_instance == null)
						CreateWindow();

					_instance._filter = null;
					_instance.SelectKey(key);
				}

				public LocalisationEditorPrefs GetEditorPrefs()
				{
					return _editorPrefs;
				}

				public void SaveEditorPrefs()
				{
					string prefsXml = Serializer.ToString(_editorPrefs);
					ProjectEditorPrefs.SetString(kEditorPrefKey, prefsXml);
				}


				private void CreateEditor()
				{
					if (_instance == null || _instance._editorPrefs == null)
					{
						_instance = (LocalisationEditorWindow)GetWindow(typeof(LocalisationEditorWindow), false, kWindowWindowName);
						_instance.Init();
					}
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
					UpdateKeys();
				}

				private void UpdateKeys()
				{
					_keys = GetKeys();
					_keysDirty = true;
				}

				private void InitGUIStyles()
				{
					if (_tableStyle == null || string.IsNullOrEmpty(_tableStyle.name))
					{
						_tableStyle = new GUIStyle(EditorStyles.toolbarTextField)
						{
							border = new RectOffset(0, 0, 0, 0),
							padding = new RectOffset(0, 0, 0, 0),
							margin = new RectOffset(0, 0, 0, 0),
							fixedHeight = 0,
						};
					}

					if (_keyStyle == null || string.IsNullOrEmpty(_keyStyle.name))
					{
						_keyStyle = new GUIStyle(EditorStyles.label)
						{
							border = new RectOffset(0, 0, 0, 0),
							padding = new RectOffset(4, 4, 4, 4),
							margin = new RectOffset(0, 0, 0, 0),
							alignment = TextAnchor.UpperLeft,
							fixedHeight = 0,
						};
					}

					if (_selectedKeyStyle == null || string.IsNullOrEmpty(_selectedKeyStyle.name))
					{
						_selectedKeyStyle = new GUIStyle(EditorStyles.whiteLabel)
						{
							border = new RectOffset(0, 0, 0, 0),
							padding = new RectOffset(4, 4, 4, 4),
							margin = new RectOffset(0, 0, 0, 0),
							fixedHeight = 0,
						};
					}

					if (_editKeyStyle == null || string.IsNullOrEmpty(_editKeyStyle.name))
					{
						_editKeyStyle = new GUIStyle(EditorStyles.textField)
						{
							border = new RectOffset(0, 0, 0, 0),
							padding = new RectOffset(4, 4, 4, 4),
							margin = new RectOffset(0, 0, 0, 0),
							fixedHeight = 0,
						};
					}

					if (_textStyle == null || string.IsNullOrEmpty(_textStyle.name))
					{
						_textStyle = new GUIStyle(EditorStyles.textField)
						{
							font = _keyStyle.font,
							fontSize = _editorPrefs._tableFontSize,
							richText = true,
							margin = new RectOffset(0, 0, 0, 0),
							border = new RectOffset(0, 0, 0, 0),
							padding = new RectOffset(4, 4, 4, 4),
							alignment = TextAnchor.MiddleLeft,
							fixedHeight = 0,
						};

						Font font = _editorPrefs._font.GetAsset();
						if (font == null)
							_textStyle.font = _keyStyle.font;
						else
							_textStyle.font = font;
					}

					if (_selectedTextStyle == null || string.IsNullOrEmpty(_selectedTextStyle.name))
					{
						_selectedTextStyle = new GUIStyle(_textStyle)
						{
							stretchHeight = false,
							wordWrap = true,
						};
					}
				}

				private void RenderTitleBar()
				{
					EditorGUILayout.BeginVertical();
					{
						//Load save
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							if (GUILayout.Button("Save", EditorStyles.toolbarButton))
							{
								Localisation.SaveStrings();
							}

							if (GUILayout.Button("Reload", EditorStyles.toolbarButton))
							{
								Localisation.ReloadStrings();
								UpdateKeys();
							}

							EditorGUILayout.Separator();

							GUILayout.Button("Scale", EditorStyles.toolbarButton);

							int fontSize = EditorGUILayout.IntSlider(_editorPrefs._tableFontSize, kMinFontSize, kMaxFontSize);

							if (GUILayout.Button("Reset Scale", EditorStyles.toolbarButton))
							{
								fontSize = kDefaultFontSize;
							}

							if (_editorPrefs._tableFontSize != fontSize)
							{
								_editorPrefs._tableFontSize = fontSize;
								_textStyle.fontSize = _editorPrefs._tableFontSize;
								_selectedTextStyle.fontSize = _editorPrefs._tableFontSize;
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

						//Filters
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.Button("Filter", EditorStyles.toolbarButton);

							EditorGUI.BeginChangeCheck();
							_filter = EditorGUILayout.TextField(_filter, EditorStyles.toolbarSearchField);
							if (EditorGUI.EndChangeCheck())
							{
								_needsRepaint = true;
								UpdateKeys();
							}

							if (GUILayout.Button("Clear", EditorStyles.toolbarButton))
							{
								_filter = "";
								UpdateKeys();
								SelectKey(_editorPrefs._selectedKeys);
							}

							if (GUILayout.Button("Choose Localisation Folder", EditorStyles.toolbarButton))
							{
								string folder = LocalisationProjectSettings.Get()._localisationFolder;
								folder = EditorUtility.OpenFolderPanel("Choose Localisation Folder", folder, "");

								if (!string.IsNullOrEmpty(folder))
								{
									LocalisationProjectSettings.Get()._localisationFolder = AssetUtils.GetAssetPath(folder);
									AssetDatabase.SaveAssets();
									Localisation.ReloadStrings(true);
									_needsRepaint = true;
								}
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						//Headers
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							float keyWidth = _editorPrefs._keyWidth - (kResizerWidth * 0.5f);
							float firstLanguageWidth = _editorPrefs._firstLanguageWidth - kResizerWidth;
							float secondLangWidth = position.width - _editorPrefs._keyWidth - _editorPrefs._firstLanguageWidth;

							keyWidth -= _scrollPosition.x;

							//Key
							if (GUILayout.Button("Key", EditorStyles.toolbarButton, GUILayout.Width(Mathf.Max(keyWidth, 0f))))
							{
								_sortOrder = _sortOrder == eKeySortOrder.Desc ? eKeySortOrder.Asc : eKeySortOrder.Desc;
							}

							//Keys Resizer
							RenderResizer(ref _keysResizerRect);

							//Current Language
							string label = "Current Language (" + Localisation.GetCurrentLanguage().ToString() + ")";
							if (GUILayout.Button(label, EditorStyles.toolbarButton, GUILayout.Width(firstLanguageWidth)))
							{
								GenericMenu menu = new GenericMenu();

								foreach (SystemLanguage lang in Enum.GetValues(typeof(SystemLanguage)))
								{
									menu.AddItem(new GUIContent(lang.ToString()), false, OnChangeCurrentLanguage, lang);
								}

								menu.ShowAsContext();
							}

							//Language Resizer
							RenderResizer(ref _languageResizerRect);

							//Second Language
							EditorGUI.BeginChangeCheck();
							SystemLanguage language = (SystemLanguage)EditorGUILayout.EnumPopup(_editorPrefs._secondLanguage, EditorStyles.toolbarPopup, GUILayout.Width(secondLangWidth));
							if (EditorGUI.EndChangeCheck())
							{
								//Unload the strings if second language is not current
								if (_editorPrefs._secondLanguage != Localisation.GetCurrentLanguage())
								{
									Localisation.UnloadStrings(_editorPrefs._secondLanguage);
								}

								_editorPrefs._secondLanguage = language;

								//If new language isn't current load strings
								if (language != Localisation.GetCurrentLanguage())
								{
									Localisation.LoadStrings(language);
								}
							}
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}

				private void RenderResizer(ref Rect rect)
				{
					GUILayout.Box(string.Empty, EditorStyles.toolbar, GUILayout.Width(kResizerWidth), GUILayout.ExpandHeight(true));
					rect = GUILayoutUtility.GetLastRect();
					EditorGUIUtility.AddCursorRect(rect, MouseCursor.SplitResizeLeftRight);
				}

				private bool IsSelected(string key)
				{
					for (int i = 0; i < _editorPrefs._selectedKeys.Length; i++)
					{
						if (_editorPrefs._selectedKeys[i] == key)
							return true;
					}

					return false;
				}

				private float GetItemHeight()
				{
					return Mathf.Max(_textStyle.CalcSize(new GUIContent("W")).y, _keyStyle.lineHeight + _keyStyle.padding.vertical);
				}

				private void RenderTable()
				{
					_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
					{
						float defaultItemHeight = GetItemHeight();
						SystemLanguage currentLanguage = Localisation.GetCurrentLanguage();
						float secondLangWidth = position.width - _editorPrefs._keyWidth - _editorPrefs._firstLanguageWidth;

						//On layout, check what part of table is currently being viewed
						if (Event.current.type == EventType.Layout || _keysDirty)
						{
							GetViewableRange(_scrollPosition.y, GetTableAreaHeight(), defaultItemHeight, out _viewStartIndex, out _viewEndIndex, out _contentStart, out _contentHeight);
						}

						EditorGUILayout.BeginVertical(GUILayout.Height(_contentHeight));
						{
							//Blank space until start of content
							GUILayout.Label(GUIContent.none, GUILayout.Height(_contentStart));

							//Then render viewable range
							for (int i = _viewStartIndex; i < _viewEndIndex; i++)
							{
								bool selected = IsSelected(_keys[i]);

								Color origBackgroundColor = GUI.backgroundColor;
								Color origContentColor = GUI.contentColor;
								
								//Work out item height
								float itemHeight;
								{
									if (selected)
									{
										//Work out highest text size
										string textA = Localisation.GetRawString(_keys[i], Localisation.GetCurrentLanguage());
										float textAHeight = _selectedTextStyle.CalcHeight(new GUIContent(textA), _editorPrefs._firstLanguageWidth);

										string textB = Localisation.GetRawString(_keys[i], _editorPrefs._secondLanguage);
										float textBHeight = _selectedTextStyle.CalcHeight(new GUIContent(textB), secondLangWidth);

										float textHeight = Mathf.Max(textAHeight, textBHeight);

										itemHeight = Mathf.Max(defaultItemHeight, textHeight);
									}
									else
									{
										itemHeight = defaultItemHeight;
									}
								}

								//Render item
								GUI.backgroundColor = selected ? kSelectedTextLineBackgroundColor : i % 2 == 0 ? kTextLineBackgroundColorA : kTextLineBackgroundColorB;
								EditorGUILayout.BeginHorizontal(_tableStyle, GUILayout.Height(itemHeight));
								{
									GUI.backgroundColor = origBackgroundColor;
									GUI.contentColor = selected ? kSelectedTextColor : Color.white;

									//Render Key
									EditorGUILayout.BeginVertical();
									{
										if (_editingKeyName == _keys[i])
										{
											EditorGUI.BeginChangeCheck();
											string key = EditorGUILayout.DelayedTextField(_keys[i], _editKeyStyle, GUILayout.Width(_editorPrefs._keyWidth), GUILayout.Height(itemHeight));
											if (EditorGUI.EndChangeCheck())
											{
												_editingKeyName = null;
												Localisation.ChangeKey(_keys[i], key);
												UpdateKeys();
											}
										}
										else
										{
											if (GUILayout.Button(_keys[i], selected ? _selectedKeyStyle : _keyStyle, GUILayout.Width(_editorPrefs._keyWidth), GUILayout.Height(itemHeight)))
											{
												OnClickItem(i, SystemLanguage.Unknown);
											}
										}
									}
									EditorGUILayout.EndVertical();

									//Render Text
									{
										GUI.backgroundColor = i % 2 == 0 ? kTextBackgroundColorA : kTextBackgroundColorB;

										//Render First Language
										string text = Localisation.GetRawString(_keys[i], currentLanguage);

										if (GUILayout.Button(selected ? text : StringUtils.GetFirstLine(text), selected ? _selectedTextStyle : _textStyle, GUILayout.Width(_editorPrefs._firstLanguageWidth), GUILayout.Height(itemHeight)))
										{
											OnClickItem(i, currentLanguage);
										}

										//Render Second Language
										EditorGUILayout.BeginVertical(GUILayout.Width(secondLangWidth));
										{
											string stext = Localisation.GetRawString(_keys[i], _editorPrefs._secondLanguage);

											if (GUILayout.Button(selected ? stext : StringUtils.GetFirstLine(stext), selected ? _selectedTextStyle : _textStyle, GUILayout.Width(secondLangWidth), GUILayout.Height(itemHeight)))
											{
												OnClickItem(i, _editorPrefs._secondLanguage);
											}
										}
										EditorGUILayout.EndVertical();
									}
								}
								EditorGUILayout.EndHorizontal();

								GUI.backgroundColor = origBackgroundColor;
								GUI.contentColor = origContentColor;
							}
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndScrollView();
				}

				private void OnClickItem(int index, SystemLanguage language)
				{
					//Add key to selection
					if (Event.current.control)
					{
						ArrayUtility.Add(ref _editorPrefs._selectedKeys, _keys[index]);
					}
					//Add keys to selection
					else if (Event.current.shift)
					{
						//Select from this key to next selected key _keys

						int nearestSelectedKeyUp = -1;
						for (int i = index; i < _keys.Length; i++)
						{
							if (IsSelected(_keys[i]))
							{
								nearestSelectedKeyUp = i;
								break;
							}
						}

						int nearestSelectedKeyDown = -1;
						for (int i = index - 1; i >= 0; i--)
						{
							if (IsSelected(_keys[i]))
							{
								nearestSelectedKeyDown = i;
								break;
							}
						}

						//No other keys selected
						if (nearestSelectedKeyUp == -1 && nearestSelectedKeyDown == -1)
						{
							_editorPrefs._selectedKeys = new string[] { _keys[index] };
						}
						else
						{
							int toUp = nearestSelectedKeyUp != -1 ? nearestSelectedKeyUp - index : int.MaxValue;
							int toDown = nearestSelectedKeyDown != -1 ? index - nearestSelectedKeyDown : int.MaxValue;

							if (toUp <= toDown)
							{
								//Add index to up to selection
								for (int i = index; i < nearestSelectedKeyUp; i++)
								{
									ArrayUtility.Add(ref _editorPrefs._selectedKeys, _keys[i]);
								}
							}
							else
							{
								//Add index to down to selection
								for (int i = index; i >= nearestSelectedKeyDown; i--)
								{
									ArrayUtility.Add(ref _editorPrefs._selectedKeys, _keys[i]);
								}
							}
						}

						//Search up from this key


						ArrayUtility.Add(ref _editorPrefs._selectedKeys, _keys[index]);
					}
					//Select just this key
					else
					{
						_editorPrefs._selectedKeys = new string[] { _keys[index] };
					}

					_editingKeyName = null;
					_needsRepaint = true;

					//If double click open up edit text window
					double time = EditorApplication.timeSinceStartup;

					if (time - lastClickTime < EditorUtils.kDoubleClickTime && index == _lastClickKeyIndex)
					{
						if (language != SystemLanguage.Unknown)
						{
							OpenTextEditor(_keys[index], language);
						}
						else
						{
							_editingKeyName = _keys[index];
						}
					}

					lastClickTime = time;
					_lastClickKeyIndex = index;
				}

				private void OpenTextEditor(string key, SystemLanguage language)
				{
					Rect position = new Rect();
					position.width = this.position.width * 0.75f;
					position.height = this.position.height * 0.33f;
					position.x = this.position.x + this.position.width * 0.125f;
					position.y = this.position.y + this.position.height * 0.33f;

					LocalisationEditorTextWindow.ShowEditKey(this, key, language, position);
				}

				private void RenderBottomBar()
				{
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					{
						GUILayout.Button("Add New Key:", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false));

						string[] folders = Localisation.GetStringFolders();
						Localisation.GetFolderIndex(_addNewKey, out int currentFolderIndex, out string keyWithoutFolder);

						EditorGUI.BeginChangeCheck();
						int newFolderIndex = EditorGUILayout.Popup(currentFolderIndex, folders, EditorStyles.toolbarDropDown);
						string currentFolder = newFolderIndex == 0 ? "" : folders[newFolderIndex];
						if (EditorGUI.EndChangeCheck())
						{
							if (newFolderIndex != 0)
								_addNewKey = currentFolder + "/" + keyWithoutFolder;
						}

						EditorGUILayout.LabelField("/", GUILayout.Width(8));

						if (newFolderIndex != 0)
						{
							keyWithoutFolder = EditorGUILayout.TextField(keyWithoutFolder, GUILayout.ExpandWidth(true));
							_addNewKey = currentFolder + "/" + keyWithoutFolder;
						}
						else
						{
							_addNewKey = EditorGUILayout.TextField(_addNewKey, GUILayout.ExpandWidth(true));
						}

						if (GUILayout.Button(" Ok ", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
						{
							if (!Localisation.Exists(_addNewKey) && !string.IsNullOrEmpty(_addNewKey))
							{
								Localisation.Set(_addNewKey, Localisation.GetCurrentLanguage(), string.Empty);
								UpdateKeys();
								SelectKey(_addNewKey);
								_addNewKey = "";
							}
						}
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
									if (_keysResizerRect.Contains(inputEvent.mousePosition))
									{
										_resizing = ResizingState.ResizingKeys;
									}
									else if (_languageResizerRect.Contains(inputEvent.mousePosition))
									{
										_resizing = ResizingState.ResizingLangauge;
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
									if (_resizing == ResizingState.ResizingKeys)
									{
										_editorPrefs._keyWidth += (inputEvent.mousePosition.x - _resizingOffset);
										_editorPrefs._keyWidth = Math.Max(_editorPrefs._keyWidth, kMinKeysWidth);
									}
									else if (_resizing == ResizingState.ResizingLangauge)
									{
										_editorPrefs._firstLanguageWidth += (inputEvent.mousePosition.x - _resizingOffset);
										_editorPrefs._firstLanguageWidth = Math.Max(_editorPrefs._firstLanguageWidth, kMinKeysWidth);
									}


									SaveEditorPrefs();
									_resizingOffset = inputEvent.mousePosition.x;
									_needsRepaint = true;
								}
							}
							break;

						case EventType.ValidateCommand:
						case EventType.ExecuteCommand:
							{
								if (inputEvent.commandName == "SoftDelete")
								{
									DeleteSelected();
								}
								else if (inputEvent.commandName == "Duplicate")
								{
									DuplicateSelected();
								}
								else if (inputEvent.commandName == "UndoRedoPerformed")
								{
									_needsRepaint = true;
								}
								else if (inputEvent.commandName == "SelectAll")
								{
									SelectAll();
								}
							}
							break;
					}
				}

				private void SelectAll()
				{
					_editorPrefs._selectedKeys = _keys;
					_needsRepaint = true;
				}

				private void DeleteSelected()
				{
					for (int i = 0; i < _editorPrefs._selectedKeys.Length; i++)
					{
						Localisation.Remove(_editorPrefs._selectedKeys[i]);
					}

					UpdateKeys();
					_editorPrefs._selectedKeys = new string[0];
					SaveEditorPrefs();
					_needsRepaint = true;
				}

				private void DuplicateSelected()
				{
					List<string> newKeys = new List<string>();

					for (int i = 0; i < _editorPrefs._selectedKeys.Length; i++)
					{
						string newKey = _editorPrefs._selectedKeys[i] + " (Copy)";

						//To do! Set text for all loaded languages?
						Localisation.Set(newKey, Localisation.GetCurrentLanguage(), Localisation.GetRawString(_editorPrefs._selectedKeys[i], Localisation.GetCurrentLanguage()));

						newKeys.Add(newKey);
					}

					UpdateKeys();
					SelectKey(newKeys.ToArray());
					_needsRepaint = true;
				}

				private void GetViewableRange(float viewStart, float viewHeight, float itemHeight, out int startIndex, out int endIndex, out float contentStart, out float contentHeight)
				{
					startIndex = _keys.Length;
					endIndex = _keys.Length;
					contentStart = 0.0f;
					contentHeight = 0.0f;

					for (int i = 0; i < _keys.Length; i++)
					{
						if (viewStart >= contentHeight && viewStart <= contentHeight + itemHeight)
						{
							startIndex = i;
							contentStart = contentHeight;
						}

						contentHeight += itemHeight;

						if (contentHeight > viewStart + viewHeight && i < endIndex)
						{
							endIndex = i + 1;
						}
					}
				}

				private void SelectKey(params string[] keys)
				{
					InitGUIStyles();

					_editorPrefs._selectedKeys = keys;
					SaveEditorPrefs();

					_needsRepaint = true;

					float toSelected = 0.0f;
					float itemHeight = GetItemHeight();
					bool foundKey = false;

					if (_editorPrefs._selectedKeys != null && _editorPrefs._selectedKeys.Length > 0)
					{
						for (int i = 0; i < _keys.Length; i++)
						{
							foundKey = _keys[i] == _editorPrefs._selectedKeys[0];

							if (foundKey)
								break;

							toSelected += itemHeight;
						}
					}

					if (foundKey)
					{
						float scrollAreaHeight = GetTableAreaHeight();
						_scrollPosition.y = Mathf.Max(toSelected - scrollAreaHeight * 0.4f, 0.0f);
					}
					else
					{
						_scrollPosition.y = 0.0f;
					}
				}

				private float GetTableAreaHeight()
				{
					return this.position.height - (EditorStyles.toolbar.fixedHeight * 4f);
				}

				private string[] GetKeys()
				{
					string[] allKeys = Localisation.GetStringKeys();
					List<string> keys = new List<string>();

					for (int i = 1; i < allKeys.Length; i++)
					{
						if (MatchsFilter(allKeys[i]) || MatchsFilter(Localisation.GetRawString(allKeys[i], Localisation.GetCurrentLanguage())))
						{
							keys.Add(allKeys[i]);
						}
					}

					switch (_sortOrder)
					{
						case eKeySortOrder.Desc:
							keys.Sort(StringComparer.InvariantCulture);
							keys.Reverse();
							break;
						default:
						case eKeySortOrder.Asc:
							keys.Sort(StringComparer.InvariantCulture);
							break;
					}

					return keys.ToArray();
				}

				private bool MatchsFilter(string text)
				{
					if (!string.IsNullOrEmpty(_filter))
					{
						//TO DO!
						//match all inside quotes
						//match all or things linked with +
						//match either of things matched by spaces

						string textLow = text.ToLower();
						string[] words = _filter.Split(' ');

						foreach (string word in words)
						{
							if (textLow.Contains(word.ToLower()))
							{
								return true;
							}
						}

						return false;
					}

					return true;
				}

				private void OnChangeCurrentLanguage(object value)
				{
					SystemLanguage language = (SystemLanguage)value;
					Localisation.SetLanguage(language);
				}
			}
		}
	}
}