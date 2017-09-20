using UnityEngine;

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
			private Text _text;
			#endregion

			#region MonoBehaviour
			void Awake()
			{
				_text = GetComponent<Text>();
			}

			void Update()
			{
				_textMesh.text = _text;
			}
			#endregion
		}
	}
}