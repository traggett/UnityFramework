using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			public sealed class LocalisationEditorTextWindow : EditorWindow
			{
				private static readonly string kWindowWindowName = "Edit Localisation String";

				private LocalisationEditorWindow _parent;
				private GUIStyle _keyStyle;
				private GUIStyle _textStyle;
				private LocalisedStringSourceAsset _item;
				private SystemLanguage _language;
				private bool _richText;	
				private bool _hasChanges;
				private Vector2 _scrollPosition;

				[SerializeField]
				private string _text;

				public static void ShowEditKey(LocalisationEditorWindow parent, LocalisedStringSourceAsset item, SystemLanguage language, Rect position)
				{
					LocalisationEditorTextWindow textEditor = (LocalisationEditorTextWindow)GetWindow(typeof(LocalisationEditorTextWindow), false, kWindowWindowName);
					textEditor.Show(parent, item, language, position);
				}

				private void Show(LocalisationEditorWindow parent, LocalisedStringSourceAsset item, SystemLanguage language, Rect position)
				{
					ShowPopup();

					this.position = position;

					_parent = parent;
					_item = item;
					_language = language;

					_keyStyle = new GUIStyle(EditorStyles.toolbarTextField)
					{
						fontStyle = FontStyle.Bold
					};

					_textStyle = new GUIStyle(EditorStyles.textArea)
					{
						font = _parent.GetEditorPrefs()._font,
						fontSize = _parent.GetEditorPrefs()._editorFontSize,
						richText = _richText,
						padding = new RectOffset(8, 8, 6, 6),
					};

					_text = _item.GetText(_language);
					_hasChanges = false;
				}

				#region EditorWindow
				void OnGUI()
				{
					EditorGUILayout.BeginVertical();
					{
						//Tool bar
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.Button(" " + _item.Key + " ", _keyStyle);

							EditorGUILayout.Separator();

							EditorGUI.BeginChangeCheck();
							SystemLanguage language = (SystemLanguage)EditorGUILayout.EnumPopup(_language, EditorStyles.toolbarPopup);
							if (EditorGUI.EndChangeCheck())
							{
								if (_hasChanges)
								{
									if (EditorUtility.DisplayDialog("Localisation String Has Been Modified", "Do you want to save the changes you made to the string?\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
									{
										_item.SetText(language, _text);
									}
								}

								_language = language;
								_text = _item.GetText(language);
								_hasChanges = false;
							}

							EditorGUI.BeginChangeCheck();
							_richText = GUILayout.Toggle(_richText, "Show Rich Text", EditorStyles.toolbarButton);
							if (EditorGUI.EndChangeCheck())
							{
								_textStyle.richText = _richText;
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						//Text scaling
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.Button("Scale", EditorStyles.toolbarButton);

							int fontSize = EditorGUILayout.IntSlider(_parent.GetEditorPrefs()._editorFontSize, LocalisationEditorWindow.kMinFontSize, LocalisationEditorWindow.kMaxFontSize);

							if (GUILayout.Button("Reset Scale", EditorStyles.toolbarButton))
							{
								fontSize = LocalisationEditorWindow.kDefaultFontSize;
							}

							if (_parent.GetEditorPrefs()._editorFontSize != fontSize)
							{
								_parent.GetEditorPrefs()._editorFontSize = fontSize;
								_textStyle.fontSize = fontSize;
								_parent.SaveEditorPrefs();
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();

						//Text
						_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, false, false);
						{
							float textHeight = _textStyle.CalcHeight(new GUIContent(_text), this.position.width);

							EditorGUI.BeginChangeCheck();
							string text = EditorGUILayout.TextArea(_text, _textStyle, GUILayout.Height(textHeight));
							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(this, "Changed Text");
								_text = text;
								_hasChanges = true;
							}
						}
						EditorGUILayout.EndScrollView();

						//Bottom bar
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.FlexibleSpace();

							if (GUILayout.Button("Save", EditorStyles.miniButton))
							{
								_item.SetText(_language, _text);
								_hasChanges = false;
								Close();
							}

							if (GUILayout.Button("Cancel", EditorStyles.miniButton))
							{
								_hasChanges = false;
								Close();
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}

				void OnDestroy()
				{
					if (_hasChanges)
					{
						if (EditorUtility.DisplayDialog("Localisation Table Has Been Modified", "Do you want to save the changes you made to the string?\nYour changes will be lost if you don't save them.", "Save", "Don't Save"))
						{
							_item.SetText(_language, _text);
						}
					}
				}
				#endregion
			}
		}
	}
}