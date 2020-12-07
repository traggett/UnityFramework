using TMPro;
using UnityEngine;

namespace Framework
{
	namespace Debug
	{
		public class DebugMenuVisuals : MonoBehaviour
		{
			public Color _highlightedColor;

			public TextMeshProUGUI _title;
			public TextMeshProUGUI[] _items;
			public int _maxItems;
		}
	}
}