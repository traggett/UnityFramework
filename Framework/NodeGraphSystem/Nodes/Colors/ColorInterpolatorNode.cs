using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	using Maths;

	namespace NodeGraphSystem
	{
		[NodeCategory("Colors")]
		[Serializable]
		public class ColorInterpolatorNode : Node, IValueSource<Color>
		{
			#region Public Data	
			public NodeInputField<Color> _from = Color.clear;
			public NodeInputField<Color> _to = Color.white;
			public NodeInputField<float> _t = 0.0f;
			public eInterpolation _interpolationtype = eInterpolation.Linear;
			#endregion

			#region IValueSource<float>
			public Color GetValue()
			{
				return MathUtils.Interpolate(_interpolationtype, _from, _to, _t);
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