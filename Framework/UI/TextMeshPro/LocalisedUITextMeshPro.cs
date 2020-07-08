using UnityEngine;
using TMPro;

namespace Framework
{
	using LocalisationSystem;

	namespace UI
	{
		namespace TextMeshPro
		{
			[ExecuteInEditMode()]
			[RequireComponent(typeof(TextMeshProUGUI))]
			public class LocalisedUITextMeshPro : MonoBehaviour
			{
				#region Public Data
				public LocalisedStringRef _text;
				#endregion

				#region Private Data 
				private TextMeshProUGUI _textMesh;
				#endregion

				#region MonoBehaviour
				private void OnEnable()
				{
					RefreshText();
				}

				private void Update()
				{
					RefreshText();
				}
				#endregion

				#region Public Methods
				public void RefreshText()
				{
					TextMeshProUGUI textMesh = GetTextMesh();
					string text = _text.GetLocalisedString();

					if (textMesh.text != text)
					{
						textMesh.text = text;
					}
				}

				public void SetVariables(params LocalisationLocalVariable[] variables)
				{
					_text.SetVariables(variables);
					RefreshText();
				}
				#endregion

				#region Private Functions
				private TextMeshProUGUI GetTextMesh()
				{
					if (_textMesh == null)
						_textMesh = GetComponent<TextMeshProUGUI>();

					return _textMesh;
				}
				#endregion
			}
		}
	}
}