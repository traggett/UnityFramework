using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class DeltaTimeNode : Node, IValueSource<float>
		{
			#region Private Data 
			private float _value;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				_value = deltaTime;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return FloatNodes.kNodeColor;
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