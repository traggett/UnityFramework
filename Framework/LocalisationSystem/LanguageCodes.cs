using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		public static class LanguageCodes
		{
			public static string GetLanguageCode(SystemLanguage language)
			{
				switch (language)
				{
					case SystemLanguage.English:
						return "en";
					case SystemLanguage.French:
						return "fr";
					case SystemLanguage.German:
						return "de";
					case SystemLanguage.Spanish:
						return "es";
					default:
						return "??";
				}
			}

			public static SystemLanguage GetLanguageFromCode(string languageCode)
			{
				if (languageCode == "en")
					return SystemLanguage.English;
				else if (languageCode == "fr")
					return SystemLanguage.French;
				else if (languageCode == "de")
					return SystemLanguage.German;
				else if (languageCode == "es")
					return SystemLanguage.Spanish;

				return SystemLanguage.Unknown;
			}
		}
	}
}