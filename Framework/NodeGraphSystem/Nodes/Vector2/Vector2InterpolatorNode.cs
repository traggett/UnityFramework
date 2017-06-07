using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;
	using Maths;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector2")]
		[Serializable]
		public class Vector2InterpolatorNode : Node, IValueSource<Vector2>
		{
			#region Public Data	
			public NodeInputField<Vector2> _from = Vector2.zero;
			public NodeInputField<Vector2> _to = Vector2.one;
			public NodeInputField<float> _t = 0.0f;
			public eInterpolation _interpolationtype = eInterpolation.Linear;
			#endregion

			#region IValueSource<Vector2>
			public Vector2 GetValue()
			{
				return MathUtils.Interpolate(_interpolationtype, _from, _to, _t);
			}
			#endregion

			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector2Nodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}