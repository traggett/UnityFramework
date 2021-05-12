using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		public abstract class LocalisedTextMesh : MonoBehaviour
		{
			public LocalisedString Text
			{
				set
				{
					_text = value;
					UpdateText(Localisation.GetCurrentLanguage());
				}
				get
				{
					return _text;
				}
			}

			#region Private Data 
			[SerializeField]
			private LocalisedString _text;
			private SystemLanguage _cachedLanguage;
			private LocalisationGlobalVariable[] _cachedGlobalVariables;
			#endregion

			#region MonoBehaviour
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
				if (language == SystemLanguage.Unknown)
					language = Localisation.GetCurrentLanguage();

				if (_cachedLanguage != language || Localisation.AreGlobalVariablesOutOfDate(_cachedGlobalVariables)
#if UNITY_EDITOR
					|| !Application.isPlaying
#endif
					)
				{
					UpdateText(language);
				}
			}

			public void SetVariables(params LocalisationLocalVariable[] variables)
			{
				_text.SetVariables(variables);
				UpdateText(Localisation.GetCurrentLanguage());
			}
			#endregion

			#region Protected Methods
			protected abstract void SetText(string text);

			protected void UpdateText(SystemLanguage language)
			{
				_cachedLanguage = language;
				_cachedGlobalVariables = Localisation.GetGlobalVariables(_text, language);
				SetText(_text.GetLocalisedString(language));
			}
			#endregion
		}
	}
}