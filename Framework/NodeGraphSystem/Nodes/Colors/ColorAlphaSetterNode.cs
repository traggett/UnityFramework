using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Colors")]
		[Serializable]
		public class ColorAlphaSetterNode : Node, IValueSource<Color>
		{
			#region Public Data	
			public NodeInputField<Color> _value = Color.clear;
			public NodeInputField<float> _alpha = 0.0f;
			#endregion

			#region Private Data
			private Color _color;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				_color = _value;
				_color.a = _alpha;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return ColorNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<Color>
			public Color GetValue()
			{
				return _color;
			}
			#endregion
		}
	}
}