using UnityEngine;

namespace Framework
{
	using DynamicValueSystem;

	namespace NodeGraphSystem
	{
		public abstract class OutputNode<TNodeField, TOutput> : Node, IValueSource<TOutput> where TNodeField : NodeInputFieldBase<TOutput>, new()
		{
			#region Public Data
			public TNodeField _input = new TNodeField();
			#endregion

			#region Private Data
			private TOutput _value;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				_value = _input;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(155.0f / 255.0f, 111.0f / 255.0f, 190.0f / 255.0f);
			}
#endif
			#endregion

			#region IValueSource
			public TOutput GetValue()
			{
				return _value;
			}
			#endregion
		}
	}
}