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
				private Vector2 _scrollPosition;
				
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
				}

				#region EditorWindow
				void OnGUI()
				{
					EditorGUILayout.BeginVertical();
					{
						//Tool bar
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.Button(" " + _item.Key + " ", EditorStyles.toolbarTextField);

							if (GUILayout.Button(EditorGUIUtility.IconContent("Clipboard"), GUILayout.Width(26f)))
							{
								GUIUtility.systemCopyBuffer = _item.Key;
							}

							EditorGUILayout.Separator();

							EditorGUI.BeginChangeCheck();
							SystemLanguage language = (SystemLanguage)EditorGUILayout.EnumPopup(_language, EditorStyles.toolbarPopup);
							if (EditorGUI.EndChangeCheck())
							{
								_language = language;
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
							string text = _item.GetText(_language);
							float textHeight = _textStyle.CalcHeight(new GUIContent(text), this.position.width);

							EditorGUI.BeginChangeCheck();
							text = EditorGUILayout.TextArea(text, _textStyle, GUILayout.Height(textHeight));
							if (EditorGUI.EndChangeCheck())
							{
								Undo.RecordObject(_item, "Edit Text");
								_item.SetText(_language, text);
							}
						}
						EditorGUILayout.EndScrollView();

						//Bottom bar
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
						{
							GUILayout.FlexibleSpace();

							if (GUILayout.Button("Ok", EditorStyles.miniButton))
							{
								Close();
							}

							GUILayout.FlexibleSpace();
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.EndVertical();
				}
				#endregion
			}
		}
	}
}