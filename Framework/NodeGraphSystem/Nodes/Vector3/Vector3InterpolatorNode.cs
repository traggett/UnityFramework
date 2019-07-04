using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	using Maths;

	namespace NodeGraphSystem
	{
		[NodeCategory("Vector3")]
		[Serializable]
		public class Vector3InterpolatorNode : Node, IValueSource<Vector3>
		{
			#region Public Data	
			public NodeInputField<Vector3> _from = Vector3.zero;
			public NodeInputField<Vector3> _to = Vector3.one;
			public NodeInputField<float> _t = 0.0f;
			public InterpolationType _interpolationtype = InterpolationType.Linear;
			#endregion

			#region IValueSource<Vector3>
			public Vector3 GetValue()
			{
				return MathUtils.Interpolate(_interpolationtype, _from, _to, _t);
			}
			#endregion
			
			#region Node
#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return Vector3Nodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}