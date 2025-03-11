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
					case SystemLanguage.Arabic:
						return "ar";
					case SystemLanguage.Afrikaans:
						return "af";
					case SystemLanguage.Basque:
						return "eu";
					case SystemLanguage.Belarusian:
						return "be";
					case SystemLanguage.Bulgarian:
						return "bg";
					case SystemLanguage.Catalan:
						return "ca";
					case SystemLanguage.Czech:
						return "cs";
					case SystemLanguage.Danish:
						return "da";
					case SystemLanguage.Dutch:
						return "nl";
					case SystemLanguage.English:
						return "en";
					case SystemLanguage.Estonian:
						return "et";
					case SystemLanguage.Faroese:
						return "fo";
					case SystemLanguage.Finnish:
						return "fi";
					case SystemLanguage.French:
						return "fr";
					case SystemLanguage.German:
						return "de";
					case SystemLanguage.Greek:
						return "el";
					case SystemLanguage.Hebrew:
						return "he";
					case SystemLanguage.Hungarian:
						return "hu";
					case SystemLanguage.Icelandic:
						return "is";
					case SystemLanguage.Indonesian:
						return "id";
					case SystemLanguage.Italian:
						return "it";
					case SystemLanguage.Japanese:
						return "ja";
					case SystemLanguage.Korean:
						return "ko";
					case SystemLanguage.Latvian:
						return "lv";
					case SystemLanguage.Lithuanian:
						return "lt";
					case SystemLanguage.Norwegian:
						return "no";
					case SystemLanguage.Polish:
						return "pl";
					case SystemLanguage.Portuguese:
						return "pt";
					case SystemLanguage.Romanian:
						return "ro";
					case SystemLanguage.Russian:
						return "ru";
					case SystemLanguage.SerboCroatian:
						return "sr";
					case SystemLanguage.Slovak:
						return "sk";
					case SystemLanguage.Slovenian:
						return "sl";
					case SystemLanguage.Spanish:
						return "es";
					case SystemLanguage.Swedish:
						return "sv";
					case SystemLanguage.Thai:
						return "th";
					case SystemLanguage.Turkish:
						return "tr";
					case SystemLanguage.Ukrainian:
						return "uk";
					case SystemLanguage.Vietnamese:
						return "vi";
					case SystemLanguage.Chinese:
					case SystemLanguage.ChineseSimplified:
					case SystemLanguage.ChineseTraditional:
						return "zh";
					case SystemLanguage.Hindi:
						return "hi";
					case SystemLanguage.Unknown:
					default:
						return "Unknown";
				}
			}

			public static SystemLanguage GetLanguageFromCode(string languageCode)
			{
				switch (languageCode.ToLower())
				{
					case "ar":
						return SystemLanguage.Arabic;
					case "af":
						return SystemLanguage.Afrikaans;
					case "eu":
						return SystemLanguage.Basque;
					case "be":
						return SystemLanguage.Belarusian;
					case "bg":
						return SystemLanguage.Bulgarian;
					case "ca":
						return SystemLanguage.Catalan;
					case "cs":
						return SystemLanguage.Czech;
					case "da":
						return SystemLanguage.Danish;
					case "nl":
						return SystemLanguage.Dutch;
					case "en":
						return SystemLanguage.English;
					case "et":
						return SystemLanguage.Estonian;
					case "fo":
						return SystemLanguage.Faroese;
					case "fi":
						return SystemLanguage.Finnish;
					case "fr":
						return SystemLanguage.French;
					case "de":
						return SystemLanguage.German;
					case "el":
						return SystemLanguage.Greek;
					case "he":
						return SystemLanguage.Hebrew;
					case "hu":
						return SystemLanguage.Hungarian;
					case "is":
						return SystemLanguage.Icelandic;
					case "id":
						return SystemLanguage.Indonesian;
					case "it":
						return SystemLanguage.Italian;
					case "ja":
						return SystemLanguage.Japanese;
					case "ko":
						return SystemLanguage.Korean;
					case "lv":
						return SystemLanguage.Latvian;
					case "lt":
						return SystemLanguage.Lithuanian;
					case "no":
						return SystemLanguage.Norwegian;
					case "pl":
						return SystemLanguage.Polish;
					case "pt":
						return SystemLanguage.Portuguese;
					case "ro":
						return SystemLanguage.Romanian;
					case "ru":
						return SystemLanguage.Russian;
					case "sr":
						return SystemLanguage.SerboCroatian;
					case "sk":
						return SystemLanguage.Slovak;
					case "sl":
						return SystemLanguage.Slovenian;
					case "es":
						return SystemLanguage.Spanish;
					case "sv":
						return SystemLanguage.Swedish;
					case "th":
						return SystemLanguage.Thai;
					case "tr":
						return SystemLanguage.Turkish;
					case "uk":
						return SystemLanguage.Ukrainian;
					case "vi":
						return SystemLanguage.Vietnamese;
					case "zh":
						return SystemLanguage.Chinese;
					case "hi":
						return SystemLanguage.Hindi;
					default:
						return SystemLanguage.Unknown;
				}
			}
		}
	}
}