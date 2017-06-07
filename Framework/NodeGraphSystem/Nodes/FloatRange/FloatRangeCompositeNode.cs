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
		public class FloatRangeCompositeNode : Node, IValueSource<FloatRange>
		{
			#region Public Data
			public NodeInputField<float> _min = 0.0f;
			public NodeInputField<float> _max = 1.0f;
			#endregion

			#region Node
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
				return new FloatRange(_min, _max);
			}
			#endregion
		}
	}
}