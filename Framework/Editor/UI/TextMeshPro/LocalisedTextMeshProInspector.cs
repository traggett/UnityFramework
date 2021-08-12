using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System;

namespace Framework
{
	using Utils;
	using LocalisationSystem;
	using LocalisationSystem.Editor;

	namespace UI
	{
		namespace TextMeshPro
		{
			namespace Editor
			{
				[CustomEditor(typeof(LocalisedTextMeshPro), true)]
				public class LocalisedTextMeshProInspector : LocalisedTextMeshInspector
				{
					private SerializedProperty _languageSettingsOverridesProp;
					private ReorderableList _languageSettingsList;
					private const float _previewButtonWidth = 120f;
					private GUIStyle _currentLanguageStyle;

					protected override void OnEnable()
					{
						base.OnEnable();

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
						_currentLanguageStyle = new GUIStyle(EditorStyles.helpBox)
						{
							alignment = TextAnchor.MiddleCenter,
							fontSize = EditorStyles.label.fontSize,
							richText = true,
						};

						DrawLocalisedTextMeshProperties();

						EditorGUI.BeginChangeCheck();

						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;

						_languageSettingsOverridesProp.isExpanded = EditorGUILayout.Foldout(_languageSettingsOverridesProp.isExpanded, "Language Override Settings");
						if (_languageSettingsOverridesProp.isExpanded)
						{
							_languageSettingsList.drawElementCallback = DrawLanguageSettings;
							_languageSettingsList.elementHeightCallback = GetLaunageSettingsHeight;
							_languageSettingsList.onAddDropdownCallback = OnAddLanguageSetting;
							_languageSettingsList.onRemoveCallback = OnRemoveLanguageSetting;

							_languageSettingsList.DoLayoutList();

							if (IsEditingOverrideSettings())
							{
								EditorGUILayout.BeginHorizontal();
								{
									if (localisedUITextMesh.EditingLanguage == SystemLanguage.Unknown)
									{
										EditorGUILayout.LabelField("Editing Text Mesh Settings for <b>" + Localisation.GetCurrentLanguage().ToString() + "</b> (Current Language)", _currentLanguageStyle);
									}
									else
									{
										EditorGUILayout.LabelField("Editing Text Mesh Settings for <b>" + localisedUITextMesh.EditingLanguage.ToString() + "</b>", _currentLanguageStyle);
									}
									
									if (localisedUITextMesh.EditingLanguage != SystemLanguage.Unknown && GUILayout.Button("Cancel"))
									{
										SwitchToEditingLanguage(SystemLanguage.Unknown);
									}
								}
								EditorGUILayout.EndHorizontal();
							}
							else
							{
								EditorGUILayout.LabelField("Editing default Text Mesh Settings", _currentLanguageStyle);
							}
						}

						if (EditorGUI.EndChangeCheck())
						{
							serializedObject.ApplyModifiedProperties();
						}
					}

					private bool IsEditingOverrideSettings()
					{
						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;
						SystemLanguage language = localisedUITextMesh.EditingLanguage;

						if (language == SystemLanguage.Unknown)
							language = Localisation.GetCurrentLanguage();

						if (localisedUITextMesh.LanguageSettingsOverrides != null)
						{
							for (int i = 0; i < localisedUITextMesh.LanguageSettingsOverrides.Length; i++)
							{
								if (localisedUITextMesh.LanguageSettingsOverrides[i]._language == language)
								{
									return true;
								}
							}
						}

						//Can't find override settings for language, edit defualts instead?
						if (localisedUITextMesh.EditingLanguage != SystemLanguage.Unknown)
							localisedUITextMesh.EditingLanguage = SystemLanguage.Unknown;

						return false;
					}

					private void OnAddLanguage(object data)
					{
						SystemLanguage language = (SystemLanguage)data;

						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;

						Undo.RecordObject(localisedUITextMesh, "Added language override");

						//Copy settings from text mesh
						TextMeshProSettings settings = TextMeshProSettings.FromTextMesh(localisedUITextMesh.TextMesh);
						LocalisedTextMeshPro.LanguageSettingsOverride languageSettingsOverride = new LocalisedTextMeshPro.LanguageSettingsOverride()
						{
							_language = language,
							_settings = settings,
						};

						//If this is the first language settings also save current settings to default
						if (localisedUITextMesh.LanguageSettingsOverrides == null || localisedUITextMesh.LanguageSettingsOverrides.Length == 0)
						{
							localisedUITextMesh.DefaultSettings = settings;
							localisedUITextMesh.LanguageSettingsOverrides = new LocalisedTextMeshPro.LanguageSettingsOverride[] { languageSettingsOverride };
						}
						//Otherwise add new settings to overrides
						else
						{
							LocalisedTextMeshPro.LanguageSettingsOverride[] languageSettingsOverrides = localisedUITextMesh.LanguageSettingsOverrides;
							ArrayUtils.Add(ref languageSettingsOverrides, languageSettingsOverride);
							localisedUITextMesh.LanguageSettingsOverrides = languageSettingsOverrides;
						}

						//Then switch to editing this new language
						SwitchToEditingLanguage(language);
					}

					private void SwitchToEditingLanguage(SystemLanguage language)
					{
						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;

						if (language == Localisation.GetCurrentLanguage())
							language = SystemLanguage.Unknown;

						//OnBeforeSerialize will save the current language settings to the correct override
						localisedUITextMesh.OnBeforeSerialize();

						//The load settings from new language
						Undo.RecordObject(localisedUITextMesh, "Edit language override");
						localisedUITextMesh.EditingLanguage = language;

						//Force the text mesh to rebuild
						EditorUtility.SetDirty(localisedUITextMesh.TextMesh);
					}

					private float GetLaunageSettingsHeight(int index)
					{
						return EditorGUIUtility.singleLineHeight;
					}

					private void DrawLanguageSettings(Rect rect, int index, bool isActive, bool isFocused)
					{
						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;

						SystemLanguage language = localisedUITextMesh.LanguageSettingsOverrides[index]._language;

						rect.width -= _previewButtonWidth;

						EditorGUI.LabelField(rect, language.ToString());

						rect.x = rect.width;
						rect.width = _previewButtonWidth;

						SystemLanguage editingLanguage = localisedUITextMesh.EditingLanguage;

						if (editingLanguage == SystemLanguage.Unknown)
						{
							editingLanguage = Localisation.GetCurrentLanguage();
						}

						if (language != editingLanguage)
						{
							if (GUI.Button(rect, "Edit"))
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
						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;

						Undo.RecordObject(localisedUITextMesh, "Removed language override");

						//If currently editing this language then revert to default
						if (localisedUITextMesh.EditingLanguage == localisedUITextMesh.LanguageSettingsOverrides[list.index]._language)
						{
							SwitchToEditingLanguage(SystemLanguage.Unknown);
						}

						//If was last lanauage clear array and clear default settings
						if (list.count == 1)
						{
							localisedUITextMesh.LanguageSettingsOverrides = null;
							localisedUITextMesh.DefaultSettings = null;
						}
						//Otherwise Remove from array
						else
						{
							LocalisedTextMeshPro.LanguageSettingsOverride[] languageSettingsOverrides = localisedUITextMesh.LanguageSettingsOverrides;
							ArrayUtils.RemoveAt(ref languageSettingsOverrides, list.index);
							localisedUITextMesh.LanguageSettingsOverrides = languageSettingsOverrides;
						}
					}

					private List<SystemLanguage> GetUnusedLanguages()
					{
						LocalisedTextMeshPro localisedUITextMesh = (LocalisedTextMeshPro)target;
						List<SystemLanguage> languages = new List<SystemLanguage>();

						foreach (SystemLanguage language in Enum.GetValues(typeof(SystemLanguage)))
						{
							if (language != SystemLanguage.Unknown)
							{
								bool alreadyExists = false;

								if (localisedUITextMesh.LanguageSettingsOverrides != null)
								{
									for (int i = 0; i < localisedUITextMesh.LanguageSettingsOverrides.Length; i++)
									{
										if (localisedUITextMesh.LanguageSettingsOverrides[i]._language == language)
										{
											alreadyExists = true;
											break;
										}
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
}