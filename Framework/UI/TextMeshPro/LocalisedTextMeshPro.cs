using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using TMPro;
using System;

namespace Framework
{
	using LocalisationSystem;
	
	namespace UI
	{
		namespace TextMeshPro
		{
			[ExecuteInEditMode()]
			[RequireComponent(typeof(TMP_Text))]
			public class LocalisedTextMeshPro : MonoBehaviour, ISerializationCallbackReceiver
			{
				#region Public Data
				public TMP_Text TextMesh
				{
					get
					{
						return _textMesh;
					}
				}

				public LocalisedString Text
				{
					set
					{
						_text = value;
						RefreshText();
					}
					get
					{
						return _text;
					}
				}

#if UNITY_EDITOR
				public SystemLanguage _editingLanguage = SystemLanguage.Unknown;
#endif
				#endregion

				#region Private Data 
				[SerializeField]
				private LocalisedString _text;
				[SerializeField]
				private TMP_Text _textMesh;
				[SerializeField]
				public TextMeshProSettings _defaultSettings;
				[Serializable]
				public class LanguageSettingsOverride
				{
					public SystemLanguage _language;
					public TextMeshProSettings _settings;
				}
				[SerializeField]
				public LanguageSettingsOverride[] _languageSettingsOverrides;
				#endregion

				#region MonoBehaviour
#if UNITY_EDITOR
				private void OnValidate()
				{
					if (_textMesh == null)
						_textMesh = GetComponent<TMP_Text>();
				}
#endif

				private void OnEnable()
				{
					SetTextMeshSettingsForLanguage();
					Localisation.OnLanguageChanged += SetTextMeshSettingsForLanguage;
					RefreshText();
				}

				private void OnDisable()
				{
					Localisation.OnLanguageChanged -= SetTextMeshSettingsForLanguage;
				}

				private void Update()
				{
#if UNITY_EDITOR
					if (_editingLanguage != SystemLanguage.Unknown)
					{
						_textMesh.text = _text.GetLocalisedString(_editingLanguage);
					}
					else
#endif
					{
						RefreshText();
					}
				}
				#endregion

				#region Public Methods
				public void RefreshText()
				{
					_textMesh.text = _text.GetLocalisedString();
				}

				public void SetVariables(params LocalisationLocalVariable[] variables)
				{
					_text.SetVariables(variables);
					RefreshText();
				}
				#endregion

				#region ISerializationCallbackReceiver
				public void OnBeforeSerialize()
				{
#if UNITY_EDITOR
					//If this text mesh has language override settings...
					if (_textMesh != null && EditorUtility.IsDirty(_textMesh) && _languageSettingsOverrides != null && _languageSettingsOverrides.Length > 0)
					{
						//...work out which langauage we're currently editing
						SystemLanguage language = _editingLanguage != SystemLanguage.Unknown ? _editingLanguage : Localisation.GetCurrentLanguage();
						bool foundLanguage = false;

						//check if we have an override for this language, if so save the current text mesh settings to that language
						for (int i = 0; i < _languageSettingsOverrides.Length; i++)
						{
							if (_languageSettingsOverrides[i]._language == language)
							{
								_languageSettingsOverrides[i]._settings = TextMeshProSettings.FromTextMesh(_textMesh);
								foundLanguage = true;
								break;
							}
						}

						//otherwise save them to default settings
						if (!foundLanguage)
						{
							_defaultSettings = TextMeshProSettings.FromTextMesh(_textMesh);
						}
					}
#endif
				}

				public void OnAfterDeserialize()
				{

				}
				#endregion

				#region Private Functions
				private void SetTextMeshSettingsForLanguage()
				{
					SetTextMeshSettingsForLanguage(Localisation.GetCurrentLanguage());
				}

				public void SetTextMeshSettingsForLanguage(SystemLanguage language)
				{
					//If this text mesh has language override settings...
					if (_languageSettingsOverrides != null && _languageSettingsOverrides.Length > 0)
					{
						//Load settings from current language
						bool foundLanguage = false;

						//check if we have an override for this language, if so load the current text mesh settings from the overrides
						for (int i = 0; i < _languageSettingsOverrides.Length; i++)
						{
							if (_languageSettingsOverrides[i]._language == language)
							{
								_languageSettingsOverrides[i]._settings.Apply(_textMesh);
								foundLanguage = true;
								break;
							}
						}

						//otherwise save them to default settings
						if (!foundLanguage)
						{
							_defaultSettings.Apply(_textMesh);
						}
					}
				}
				#endregion
			}
		}
	}
}