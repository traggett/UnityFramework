using System;
using System.Collections.Generic;

using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[Serializable]
		public struct LocalisedStringRef
		{
			#region Serialized Data
			[SerializeField]
			private string _localisationKey;
			#endregion

			#region Private Data
			private string _cachedText;
			private SystemLanguage _cachedLanguage;
			private LocalisationVariableInfo[] _cachedVariables;
			#endregion

			#region Editor Data
#if UNITY_EDITOR
			[NonSerialized]
			public bool _editorCollapsed;	
			private string _editorAutoNameParentName;
#endif
			#endregion
			
			public LocalisedStringRef(string key)
			{
				_localisationKey = key;
				_cachedText = string.Empty;
				_cachedLanguage = SystemLanguage.Unknown;
				_cachedVariables = null;
#if UNITY_EDITOR
				_editorCollapsed = false;
				_editorAutoNameParentName = null;
#endif
			}

			public static implicit operator string(LocalisedStringRef property)
			{
				return property.GetLocalisedString();
			}

			public static implicit operator LocalisedStringRef(string key)
			{
				return new LocalisedStringRef(key);
			}
			
			//Check variables are updated, if so update variables name
			//how can we check if a variable has been updated?
			//Surely its better to cache text in one place??
			//nah that wont work
			//so instead somehow need to get told a variable updates
			//maybe provide interface that registers itself?
			//get string with interface - all variables in that string are linked to it?
			//or localised string somehow queues if a variable is dirty??

			//anywhere in code calls setvariable

			//any where in code alls get localised string

			//localisation updates

			//each variable could have a key and a version number
			//whenever you update a variable that nunmber changes
			//


			public string GetLocalisedString()
			{
				if (_cachedLanguage != Localisation.GetCurrentLanguage() || Localisation.AreVariablesOutOfDate(_cachedVariables))
				{
					_cachedLanguage = Localisation.GetCurrentLanguage();
					_cachedText = Localisation.GetString(_localisationKey);
					_cachedVariables = Localisation.GetVariablesKeys(_cachedText);
				}

				return _cachedText;
			}

#if UNITY_EDITOR
			public string GetLocalisationKey()
			{
				return _localisationKey;
			}

			public SystemLanguage GetDebugLanguage()
			{
				return _cachedLanguage;
			}

			public string GetDebugText()
			{
				return _cachedText;
			}

			public void SetAutoNameParentName(string parentName)
			{
				_editorAutoNameParentName = parentName;
			}
			
			public string GetAutoNameParentName()
			{
				return _editorAutoNameParentName;
			}

			public string GetAutoKey()
			{
				string autoKey = null;
				
				if (!string.IsNullOrEmpty(_editorAutoNameParentName))
				{
					string autoNameParent = _editorAutoNameParentName;

					//Replace _ with / so each bit will appear in separate dropdown menu (eg TextConv_Hath_Birthday will go to TextConv, Hath, Birthday)
					autoNameParent = autoNameParent.Replace('_', '/');

					//Find first free key
					int index = 0;
					while (Localisation.IsKeyInTable(autoKey = autoNameParent + "/" + index.ToString("000")))
					{
						index++;
					}
				}

				return autoKey;
			}		
#endif
		}
	}
}