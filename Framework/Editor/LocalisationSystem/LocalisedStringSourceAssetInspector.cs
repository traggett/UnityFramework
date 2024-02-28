using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedStringSourceAsset), true)]
			public class LocalisedStringSourceAssetInspector : UnityEditor.Editor
			{
				private GUIStyle _textAreaStyle;

				public override void OnInspectorGUI()
				{
					LocalisedStringSourceAsset sourceAsset = (LocalisedStringSourceAsset)target;

					// Show full key
					EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
					{
						GUILayout.Button(" " + sourceAsset.Key + " ", EditorStyles.toolbarTextField, GUILayout.ExpandWidth(true));

						if (GUILayout.Button(EditorGUIUtility.IconContent("Clipboard"), GUILayout.Width(26f)))
						{
							GUIUtility.systemCopyBuffer = sourceAsset.Key;
						}
					}
					GUILayout.EndHorizontal();

					EditorGUILayout.Separator();

					// Show each language
					{
						DrawLanguage(sourceAsset, Application.systemLanguage);

						SystemLanguage[] languages = sourceAsset.GetLanguages();

						for (int i = 0; i < languages.Length; i++)
						{
							if (languages[i] != Application.systemLanguage)
							{
								DrawLanguage(sourceAsset, languages[i]);
							}
						}
					}
					
				}

				private void DrawLanguage(LocalisedStringSourceAsset sourceAsset, SystemLanguage language)
				{
					if (_textAreaStyle == null)
					{
						_textAreaStyle = new GUIStyle(EditorStyles.toolbarTextField);
						_textAreaStyle.stretchWidth = true;
						_textAreaStyle.stretchHeight = true;
						_textAreaStyle.fixedHeight = 0;
					}


					EditorGUILayout.BeginVertical(EditorStyles.helpBox);
					{
						// Show full key
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField(language.ToString());

							if (GUILayout.Button("Edit"))
							{
								LocalisationEditorWindow.EditString(sourceAsset, language);
							}
						}
						EditorGUILayout.EndHorizontal();

						string text = sourceAsset.GetText(language);

						//Show preview text and edit buttonRect textPosition = new Rect(position.x + labelWidth, yPos, position.width - labelWidth - _editbuttonWidth - _buttonSpace, height);
						//EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.TextArea(text, _textAreaStyle);
						//EditorGUI.EndDisabledGroup();
					}
					EditorGUILayout.EndVertical();
				}
			}
		}
	}
}