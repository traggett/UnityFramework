using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	using Maths;

	namespace NodeGraphSystem
	{
		[NodeCategory("Quaternions")]
		[Serializable]
		public class QuaternionInterpolatorNode : Node, IValueSource<Quaternion>
		{
			#region Public Data	
			public NodeInputField<Quaternion> _from = Quaternion.identity;
			public NodeInputField<Quaternion> _to = Quaternion.identity;
			public NodeInputField<float> _t = 0.0f;
			public eInterpolation _interpolationtype = eInterpolation.Linear;
			#endregion

			#region IValueSource<Quaternion>
			public Quaternion GetValue()
			{
				return MathUtils.Interpolate(_interpolationtype, _from, _to, _t);
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return QuaternionNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}