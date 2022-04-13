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
		public class FloatRangeCompositeNode : Node, IValueSource<FloatRange>
		{
			#region Public Data
			public NodeInputField<float> _min = 0.0f;
			public NodeInputField<float> _max = 1.0f;
			#endregion

			#region Private Data
			private FloatRange _floatRange;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				_floatRange = new FloatRange(_min, _max);
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatRangeNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<FloatRange>
			public FloatRange GetValue()
			{
				return _floatRange;
			}
			#endregion
		}
	}
}