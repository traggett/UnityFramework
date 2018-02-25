using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	namespace LocalisationSystem
	{
		[ExecuteInEditMode()]
		[RequireComponent(typeof(Text))]
		public class LocalisedUITextMesh : MonoBehaviour
		{
			#region Public Data
			public LocalisedStringRef _text;
			#endregion

			#region Private Data 
			private Text _UIText;
			#endregion

			#region MonoBehaviour
			void Awake()
			{
				_UIText = GetComponent<Text>();
			}

			void Update()
			{
				string text = _text.GetLocalisedString();

				if (_UIText.text != text)
				{
					_UIText.text = text;
				}
			}
			#endregion
		}
	}
}