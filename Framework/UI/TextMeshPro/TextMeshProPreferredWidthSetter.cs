using UnityEngine;
using TMPro;

namespace Framework
{
	namespace UI
	{
		namespace TextMeshPro
		{
			[RequireComponent(typeof(TextMeshProUGUI))]
			[ExecuteInEditMode]
			public class TextMeshProPreferredWidthSetter : MonoBehaviour
			{
				#region Private Data
				private TextMeshProUGUI _textMesh;
				#endregion

				#region Unity Messages
				private void OnEnable()
				{
					if (_textMesh == null)
						_textMesh = GetComponent<TextMeshProUGUI>();

					UpdateWidth();
				}

				private void LateUpdate()
				{
					UpdateWidth();
				}
				#endregion

				#region Private Functions
				private void UpdateWidth()
				{
					RectTransformUtils.SetWidth(_textMesh.rectTransform, _textMesh.preferredWidth);
				}
				#endregion
			}
		}
	}
}