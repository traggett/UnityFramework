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

			#region IValueSource<float>
			public Color GetValue()
			{
				Gradient gradient = _gradient;

				if (gradient != null)
				{
					return gradient.Evaluate(_t);
				}

				return Color.black;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return ColorNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}