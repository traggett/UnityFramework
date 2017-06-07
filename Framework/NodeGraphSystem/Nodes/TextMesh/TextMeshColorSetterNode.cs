using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("TextMesh")]
		[Serializable]
		public class TextMeshColorSetterNode : Node
		{
			#region Public Data
			public ComponentNodeInputField<TextMesh> _textMesh = new ComponentNodeInputField<TextMesh>();
			public NodeInputField<Color> _value = Color.clear;
			#endregion

			#region Private Data
			private Color _cachedValue;
			#endregion

			#region Node
			public override void Update(float deltaTime)
			{
				TextMesh textMesh = _textMesh;
				Color value = _value;

				textMesh.color = value;
			}
			#endregion
		}
	}
}