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
			public LocalisedString _text;
			#endregion

			#region Private Data 
			private TextMesh _textMesh;
			#endregion

			#region MonoBehaviour
			void Awake()
			{
				_textMesh = GetComponent<TextMesh>();
				RefreshText();
			}

			void Update()
			{
				RefreshText();
			}
			#endregion

			#region Private Methods
			public void RefreshText()
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