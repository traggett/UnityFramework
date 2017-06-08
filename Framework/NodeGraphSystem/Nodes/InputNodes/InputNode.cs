using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		public abstract class InputNode<T> : Node, IValueSource<T>
		{
			[HideInInspector]
			public IValueSource<T> _inputSource;

			#region Node
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
				if (_inputSource != null)
					return _inputSource.GetValue();

				return default(T);
			}
			#endregion
			
			public void SetInputSource<TValueSource>(TValueSource obj) where TValueSource : class, IValueSource<T>
			{
				_inputSource = obj;
			}
		}
	}
}