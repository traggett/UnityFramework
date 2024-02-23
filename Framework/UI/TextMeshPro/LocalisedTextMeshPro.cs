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
			public class LocalisedTextMeshPro : LocalisedTextMesh, ISerializationCallbackReceiver
			{
				#region Public Data
				public TMP_Text TextMesh
				{
					get
					{
						if (this != null && _textMesh == null)
						{
							_textMesh = GetComponent<TMP_Text>();
						}

						return _textMesh;
					}
				}
				#endregion

				#region Private Data 
				private TMP_Text _textMesh;
				[SerializeField]
				private TextMeshProSettings _defaultSettings;
				[Serializable]
				public class LanguageSettingsOverride
				{
					public SystemLanguage _language;
					public TextMeshProSettings _settings;
				}
				[SerializeField]
				private LanguageSettingsOverride[] _languageSettingsOverrides;
#if UNITY_EDITOR
				private SystemLanguage _editingLanguage = SystemLanguage.Unknown;
#endif
				#endregion

				#region Editor Only Properties
#if UNITY_EDITOR
				public SystemLanguage EditingLanguage
				{
					get
					{
						return _editingLanguage;
					}
					set
					{
						_editingLanguage = value;
						SetTextMeshSettingsForLanguage(value);
					}
				}

				public TextMeshProSettings DefaultSettings
				{
					get
					{
						return _defaultSettings;
					}
					set
					{
						_defaultSettings = value;
					}
				}

				public LanguageSettingsOverride[] LanguageSettingsOverrides
				{
					get
					{
						return _languageSettingsOverrides;
					}
					set
					{
						_languageSettingsOverrides = value;
					}
				}
#endif
				#endregion

				#region Unity Messages
				protected override void OnEnable()
				{
					SetTextMeshSettingsForLanguage();
					Localisation.OnLanguageChanged += SetTextMeshSettingsForLanguage;
					RefreshText();
				}

				private void OnDisable()
				{
					Localisation.OnLanguageChanged -= SetTextMeshSettingsForLanguage;
				}

				protected override void Update()
				{
#if UNITY_EDITOR
					RefreshText(_editingLanguage);
#else
					RefreshText();
#endif
				}
				#endregion

				#region LocalisedTextMesh
				protected override void SetText(string text)
				{
					TextMesh.text = text;
				}
				#endregion

				#region ISerializationCallbackReceiver
				public void OnBeforeSerialize()
				{
#if UNITY_EDITOR
					//If this text mesh has language override settings...
					if (EditorUtility.IsDirty(TextMesh) && _languageSettingsOverrides != null && _languageSettingsOverrides.Length > 0)
					{
						//...work out which langauage we're currently editing
						SystemLanguage language = _editingLanguage;
						
						if (language == SystemLanguage.Unknown)
							language = Localisation.GetCurrentLanguage();

						bool foundLanguage = false;

						//check if we have an override for this language, if so save the current text mesh settings to that language
						for (int i = 0; i < _languageSettingsOverrides.Length; i++)
						{
							if (_languageSettingsOverrides[i]._language == language)
							{
								_languageSettingsOverrides[i]._settings = TextMeshProSettings.FromTextMesh(TextMesh);
								foundLanguage = true;
								break;
							}
						}

						//otherwise save them to default settings
						if (!foundLanguage)
						{
							_defaultSettings = TextMeshProSettings.FromTextMesh(TextMesh);
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

				private void SetTextMeshSettingsForLanguage(SystemLanguage language)
				{
					if (language == SystemLanguage.Unknown)
						language = Localisation.GetCurrentLanguage();

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
								_languageSettingsOverrides[i]._settings.Apply(TextMesh);
								foundLanguage = true;
								break;
							}
						}

						//otherwise save them to default settings
						if (!foundLanguage)
						{
							_defaultSettings.Apply(TextMesh);
						}
					}
				}
				#endregion
			}
		}
	}
}