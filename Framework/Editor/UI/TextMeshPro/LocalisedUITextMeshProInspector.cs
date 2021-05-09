using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Framework.Utils;
using Framework.UI.TextMeshPro;
using System.Collections.Generic;
using System;

namespace Framework
{
	namespace Paths
	{
		namespace Editor
		{
			[CustomEditor(typeof(LocalisedUITextMeshPro), true)]
			public class LocalisedUITextMeshProInspector : UnityEditor.Editor
			{
				private SerializedProperty _textProp;
				private SerializedProperty _languageSettingsOverridesProp;
				private ReorderableList _languageSettingsList;
				private const float _previewButtonWidth = 120f;
				private GUIStyle _currentLanguageStyle;

				private void OnEnable()
				{
					_textProp = serializedObject.FindProperty("_text");
					_languageSettingsOverridesProp = serializedObject.FindProperty("_languageSettingsOverrides");
					_languageSettingsList = new ReorderableList(serializedObject, _languageSettingsOverridesProp, true, false, true, true)
					{
						showDefaultBackground = true,
						headerHeight = 4f,
						index = 0,
						elementHeight = 20f
					};
				}

				public override void OnInspectorGUI()
				{
					EditorGUI.BeginChangeCheck();

					LocalisedUITextMeshPro localisedUITextMesh = (LocalisedUITextMeshPro)target;

					EditorGUILayout.PropertyField(_textProp);

					if (_currentLanguageStyle == null)
					{
						_currentLanguageStyle = new GUIStyle(EditorStyles.helpBox)
						{
							alignment = TextAnchor.MiddleCenter,
							fontSize = EditorStyles.label.fontSize,
							richText = true,
						};
					}

					_languageSettingsOverridesProp.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(_languageSettingsOverridesProp.isExpanded, "Language Override Settings");
					if (_languageSettingsOverridesProp.isExpanded)
					{
						_languageSettingsList.drawElementCallback = DrawLanguageSettings;
						_languageSettingsList.elementHeightCallback = GetLaunageSettingsHeight;
						_languageSettingsList.onAddDropdownCallback = OnAddLanguageSetting;
						_languageSettingsList.onRemoveCallback = OnRemoveLanguageSetting;

						_languageSettingsList.DoLayoutList();

						if (localisedUITextMesh._editingLanguage != SystemLanguage.Unknown)
						{
							EditorGUILayout.BeginHorizontal();
							{
								EditorGUILayout.LabelField("Viewing Text Mesh Settings for <b>" + localisedUITextMesh._editingLanguage.ToString() + "</b>", _currentLanguageStyle);

								if (GUILayout.Button("Cancel"))
								{
									SwitchToEditingLanguage(SystemLanguage.Unknown);
								}
							}
							EditorGUILayout.EndHorizontal();
						}
					}
					EditorGUILayout.EndFoldoutHeaderGroup();

					if (EditorGUI.EndChangeCheck())
					{
						serializedObject.ApplyModifiedProperties();
					}
				}
				private void OnAddLanguage(object data)
				{
					SystemLanguage language = (SystemLanguage)data;

					LocalisedUITextMeshPro localisedUITextMesh = (LocalisedUITextMeshPro)target;

					Undo.RecordObject(localisedUITextMesh, "Added language override");

					//Copy settings from text mesh
					TextMeshProSettings settings = TextMeshProSettings.FromTextMesh(localisedUITextMesh.TextMesh);
					LocalisedUITextMeshPro.LanguageSettingsOverride languageSettingsOverride = new LocalisedUITextMeshPro.LanguageSettingsOverride()
					{
						_language = language,
						_settings = settings,
					};

					//If this is the first language settings also save current settings to default
					if (localisedUITextMesh._languageSettingsOverrides == null || localisedUITextMesh._languageSettingsOverrides.Length == 0)
					{
						localisedUITextMesh._defaultSettings = settings;
						localisedUITextMesh._languageSettingsOverrides = new LocalisedUITextMeshPro.LanguageSettingsOverride[] { languageSettingsOverride };
					}
					//Otherwise add new settings to overrides
					else
					{
						ArrayUtils.Add(ref localisedUITextMesh._languageSettingsOverrides, languageSettingsOverride);
					}

					//Then switch to editing this new language
					SwitchToEditingLanguage(language);
				}

				private void SwitchToEditingLanguage(SystemLanguage language)
				{
					LocalisedUITextMeshPro localisedUITextMesh = (LocalisedUITextMeshPro)target;
					
					//OnBeforeSerialize will save the current language settings to the correct override
					localisedUITextMesh.OnBeforeSerialize();

					//The load settings from new language
					Undo.RecordObject(localisedUITextMesh, "Edit language override");
					localisedUITextMesh._editingLanguage = language;
					localisedUITextMesh.SetTextMeshSettingsForLanguage(language);

					//Force the text mesh to rebuild
					EditorUtility.SetDirty(localisedUITextMesh.TextMesh);
				}

				private float GetLaunageSettingsHeight(int index)
				{
					return EditorGUIUtility.singleLineHeight;
				}

				private void DrawLanguageSettings(Rect rect, int index, bool isActive, bool isFocused)
				{
					SystemLanguage language = (SystemLanguage)_languageSettingsOverridesProp.GetArrayElementAtIndex(index).FindPropertyRelative("_language").intValue;

					rect.width -= _previewButtonWidth;

					EditorGUI.LabelField(rect, language.ToString());

					rect.x = rect.width;
					rect.width = _previewButtonWidth;

					LocalisedUITextMeshPro localisedUITextMesh = (LocalisedUITextMeshPro)target;
					
					if (localisedUITextMesh._editingLanguage != language)
					{
						if (GUI.Button(rect, "Preview"))
						{
							SwitchToEditingLanguage(language);
						}
					}
				}

				private void OnAddLanguageSetting(Rect buttonRect, ReorderableList list)
				{
					GenericMenu menu = new GenericMenu();

					List<SystemLanguage> languages = GetUnusedLanguages();

					foreach (SystemLanguage language in languages)
					{
						menu.AddItem(new GUIContent(language.ToString()), false, OnAddLanguage, language);
					}

					menu.ShowAsContext();
				}

				private void OnRemoveLanguageSetting(ReorderableList list)
				{
					LocalisedUITextMeshPro localisedUITextMesh = (LocalisedUITextMeshPro)target;

					Undo.RecordObject(localisedUITextMesh, "Removed language override");

					//If currently editing this language then revert to default
					if (localisedUITextMesh._editingLanguage == localisedUITextMesh._languageSettingsOverrides[list.index]._language)
					{
						SwitchToEditingLanguage(SystemLanguage.Unknown);
					}

					//If was last lanauage clear array and clear default settings
					if (list.count == 1)
					{
						localisedUITextMesh._languageSettingsOverrides = null;
						localisedUITextMesh._defaultSettings = null;
					}
					//Otherwise Remove from array
					else
					{
						ArrayUtils.RemoveAt(ref localisedUITextMesh._languageSettingsOverrides, list.index);
					}
				}

				private List<SystemLanguage> GetUnusedLanguages()
				{
					List<SystemLanguage> languages = new List<SystemLanguage>();

					foreach (SystemLanguage language in Enum.GetValues(typeof(SystemLanguage)))
					{
						if (language != SystemLanguage.Unknown)
						{
							bool alreadyExists = false;

							for (int i = 0; i < _languageSettingsOverridesProp.arraySize; i++)
							{
								if (_languageSettingsOverridesProp.GetArrayElementAtIndex(i).FindPropertyRelative("_language").intValue == (int)language)
								{
									alreadyExists = true;
									break;
								}
							}

							if (!alreadyExists)
							{
								languages.Add(language);
							}
						}
					}

					return languages;
				}
			}
		}
	}
}