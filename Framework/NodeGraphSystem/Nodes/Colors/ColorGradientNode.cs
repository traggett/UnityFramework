using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;	

	namespace NodeGraphSystem
	{
		[NodeCategory("Colors")]
		[Serializable]
		public class ColorGradientNode : Node, IValueSource<Color>
		{
			#region Public Data	
			public NodeInputField<Gradient> _gradient;
			public NodeInputField<float> _t = 0.0f;
			#endregion

			#region Private Data
			private Color _color;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				Gradient gradient = _gradient;

				if (gradient != null)
				{
					_color = gradient.Evaluate(_t);
				}
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