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
			public sealed class LocalisationEditorTextWindow : EditorWindow
			{
				private static readonly string kWindowWindowName = "Edit Localisation String";
				
				private GUIStyle _localiserTextStyle;
				private GUIStyle _textStyle;
				private string _key;
				private SystemLanguage _language;
				private bool _richText;	
				private bool _hasChanges;
				private Vector2 _scrollPosition;

				[SerializeField]
				private string _text;

				public static void ShowEditKey(string key, SystemLanguage language, GUIStyle style, Rect position)
				{
					LocalisationEditorTextWindow textEditor = (LocalisationEditorTextWindow)GetWindow(typeof(LocalisationEditorTextWindow), false, kWindowWindowName);
					textEditor.Show(key, language, style, position);
				}

				public void Show(string key, SystemLanguage language, GUIStyle style, Rect position)
				{
					ShowPopup();

					this.position = position;
					_key = key;
					_language = language;
					_localiserTextStyle = style;

					_textStyle = new GUIStyle(EditorStyles.textArea);
					_textStyle.font = _localiserTextStyle.font;
					_textStyle.fontSize = _localiserTextStyle.fontSize;
					_textStyle.richText = _richText;

					_text = Localisation.GetRawString(_key, _language);
					_hasChanges = false;
				}

				#region EditorWindow
				void OnGUI()
				{
					EditorGUILayout.BeginVertical();
					{
						//Tool bar
						EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(EditorGUIUtility.singleLineHeight));
						{
							if (GUILayout.Button(_key, EditorStyles.objectField))
							{

							}

							EditorGUI.BeginChangeCheck();
							_richText = GUILayout.Toggle(_richText, "Rich Text", EditorStyles.toolbarButton);
							if (EditorGUI.EndChangeCheck())
							{
								_textStyle.richText = _richText;
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
						EditorGUILayout.BeginHorizontal(GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f));
						{
							GUILayout.FlexibleSpace();

							if (GUILayout.Button("Ok", EditorStyles.miniButton))
							{
								Localisation.Set(_key, _language, _text);
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
							Localisation.Set(_key, _language, _text);
						}
					}
				}
				#endregion
			}
		}
	}
}