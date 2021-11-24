using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	namespace LocalisationSystem
	{
		[ExecuteInEditMode()]
		[RequireComponent(typeof(Text))]
		public class LocalisedUITextMesh : LocalisedTextMesh
		{
			#region Public Data
			public Text TextMesh
			{
				get
				{
					return _textMesh;
				}
			}
			#endregion

			#region Private Data 
			[SerializeField]
			private Text _textMesh;
			#endregion

			#region Unity Messages
#if UNITY_EDITOR
			private void OnValidate()
			{
				if (_textMesh == null)
					_textMesh = GetComponent<Text>();
			}
#endif
			#endregion

			#region LocalisedTextMesh
			protected override void SetText(string text)
			{
				_textMesh.text = text;
			}
			#endregion
		}
	}
}