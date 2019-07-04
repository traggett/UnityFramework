using UnityEngine;
using System;

namespace Framework
{
	using Maths;
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("FloatRange")]
		[Serializable]
		public class EvaluateFloatRangeNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _input = 0.0f;
			public NodeInputField<FloatRange> _range = new FloatRange();
			public InterpolationType _easeType = InterpolationType.Linear;
			#endregion

			#region Private Data
			private float _value;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				FloatRange range = _range;
				float lerp = MathUtils.Interpolate(_easeType, 0.0f, 1.0f, _input);
				_value = range.Get(lerp);
			}

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
				return _value;
			}
			#endregion
		}
	}
}