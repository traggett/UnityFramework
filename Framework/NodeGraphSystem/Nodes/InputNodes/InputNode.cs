using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		public abstract class InputNode<T> : Node, IValueSource<T>
		{
			#region Public Data
			[HideInInspector]
			public IValueSource<T> _inputSource;
			#endregion

			#region Private Data
			private T _value;
			#endregion

			#region Node
			public override void UpdateNode(float time, float deltaTime)
			{
				if (_inputSource != null)
					_value = _inputSource.GetValue();
				else
					_value = default(T);
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(113 / 255.0f, 229 / 255.0f, 98/255.0f);
			}
#endif
			#endregion

			#region IValueSource<T>
			public T GetValue()
			{
				return _value;
			}
			#endregion
			
			public void SetInputSource<TDynamicValue>(TDynamicValue obj) where TDynamicValue : class, IValueSource<T>
			{
				_inputSource = obj;
			}
		}
	}
}