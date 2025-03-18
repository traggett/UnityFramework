using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		public abstract class LocalisedTextMesh : MonoBehaviour
		{
			#region Public Data 
			public LocalisedString Text
			{
				set
				{
					_text = value;
					_textListMode = false;
					UpdateText(Localisation.GetCurrentLanguage());
				}
				get
				{
					return _text;
				}
			}

			#region List Data
			public LocalisedString[] TextList
			{
				set
				{
					_textList = value;
					_textListMode = true;
					UpdateText(Localisation.GetCurrentLanguage());
				}
				get
				{
					return _textList;
				}
			}

			public LocalisedString _listStartString;
			public LocalisedString _listSeperatorString;
			public LocalisedString _listEndString;
			#endregion

			#endregion

			#region Serialized Data 
			[SerializeField] private LocalisedString _text;

			[SerializeField]
			[Tooltip("List Mode Allows multiple localised strings to be concated whilst staying localised.")]
			private bool _textListMode;

			[SerializeField]
			private LocalisedString[] _textList;

			[SerializeField] private bool _forceLanguage;
			[SerializeField] private SystemLanguage _forcedLanguage;
			#endregion

			#region Private Data 
			private SystemLanguage _cachedLanguage;
			private LocalisationGlobalVariable[] _cachedGlobalVariables;

#if UNITY_EDITOR
			private int _cachedEditorVersion;
#endif
			#endregion

			#region Unity Messages
			protected virtual void OnEnable()
			{
				RefreshText();
			}

			protected virtual void Update()
			{
				RefreshText();
			}
			#endregion

			#region Public Methods
			public void RefreshText(SystemLanguage language = SystemLanguage.Unknown)
			{
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					UpdateTextEditor(language);
					return;
				}
#endif

				if (language == SystemLanguage.Unknown)
				{
					if (_forceLanguage)
					{
						language = _forcedLanguage;
					}
					else
					{
						language = Localisation.GetCurrentLanguage();
					}
				}
				
				if (_cachedLanguage != language || Localisation.AreGlobalVariablesOutOfDate(_cachedGlobalVariables))
				{
					UpdateText(language);
				}
			}

			public void SetVariables(params LocalisationLocalVariable[] variables)
			{
				if (_textListMode)
				{
					UnityEngine.Debug.LogWarning("SetVariables isn't supported in list mode.");
				}
				else
				{
					_text.SetVariables(variables);
					UpdateText(Localisation.GetCurrentLanguage());
				}
			}
			#endregion

			#region Protected Methods
			protected abstract void SetText(string text);


			// When not playing (in editor mode) update text directly table asset not Localisation

			protected void UpdateText(SystemLanguage language)
			{
				_cachedLanguage = language;

				//List of strings
				if (_textListMode)
				{
					_cachedGlobalVariables = Localisation.GetGlobalVariables();

					//Build text from list
					string text = _listStartString;

					for (int i = 0; i < _textList.Length; i++)
					{
						text += _textList[i].GetLocalisedString(language);

						if (i < _textList.Length - 1)
						{
							text += _listSeperatorString;
						}
					}

					text += _listEndString;

					SetText(text);
				}
				//Simple string
				else
				{
					_cachedGlobalVariables = Localisation.GetGlobalVariables(_text.GetLocalisationKey(), language);
					SetText(_text.GetLocalisedString(language));
				}
			}
#if UNITY_EDITOR
			protected void UpdateTextEditor(SystemLanguage language)
			{
				if (language == SystemLanguage.Unknown)
				{
					if (_forceLanguage)
					{
						language = _forcedLanguage;
					}
					else
					{
						language = Localisation.GetCurrentLanguage();
					}
				}

				//List of strings
				if (_textListMode)
				{
					//Build text from list
					string text = _listStartString;

					for (int i = 0; i < _textList.Length; i++)
					{
						text += _textList[i].GetLocalisedString(language);

						if (i < _textList.Length - 1)
						{
							text += _listSeperatorString;
						}
					}

					text += _listEndString;

					SetText(text);
				}
				//Simple string
				else
				{
					SetText(_text.GetLocalisedString(language));
				}
			}
#endif
			#endregion
		}
	}
}