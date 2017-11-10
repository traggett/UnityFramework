using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Float")]
		[Serializable]
		public class FloatAbsNode : Node, IValueSource<float>
		{
			#region Public Data
			public NodeInputField<float> _value = 0.0f;
			#endregion

			#region Private Data
			private float _absValue;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				_absValue = Mathf.Abs(_value);
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
				return _absValue;
			}
			#endregion
		}
	}
}