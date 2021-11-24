using UnityEngine;

namespace Framework
{
	namespace LocalisationSystem
	{
		[ExecuteInEditMode()]
		[RequireComponent(typeof(TextMesh))]
		public class Localised3DTextMesh : LocalisedTextMesh
		{
			#region Public Data
			public TextMesh TextMesh
			{
				get
				{
					return _textMesh;
				}
			}
			#endregion

			#region Private Data 
			[SerializeField]
			private TextMesh _textMesh;
			#endregion

			#region Unity Messages
#if UNITY_EDITOR
			private void OnValidate()
			{
				if (_textMesh == null)
					_textMesh = GetComponent<TextMesh>();
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