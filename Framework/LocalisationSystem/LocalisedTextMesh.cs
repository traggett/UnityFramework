using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[ExecuteInEditMode()]
		[RequireComponent(typeof(TextMesh))]
		public class LocalisedTextMesh : MonoBehaviour
		{
			#region Public Data
			public LocalisedStringRef _text;
			#endregion

			#region Private Data 
			private TextMesh _textMesh;
			#endregion

			#region MonoBehaviour
			void Awake()
			{
				_textMesh = GetComponent<TextMesh>();
			}

			void Update()
			{
				string text = _text.GetLocalisedString();

				if (_textMesh.text != text)
				{
					_textMesh.text = text;
				}
			}
			#endregion
		}
	}
}