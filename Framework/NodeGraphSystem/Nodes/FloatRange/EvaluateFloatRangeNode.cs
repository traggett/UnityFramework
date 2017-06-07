using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("FloatRange")]
		[Serializable]
		public class EvaluateFloatRangeNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _input = 0.0f;
			public NodeInputField<FloatRange> _range = new FloatRange();
			public eInterpolation _easeType = eInterpolation.Linear;
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatRangeNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				FloatRange range = _range;
				float lerp = MathUtils.Interpolate(_easeType, 0.0f, 1.0f, _input);
				return range.Get(lerp);
			}
			#endregion
		}
	}
}