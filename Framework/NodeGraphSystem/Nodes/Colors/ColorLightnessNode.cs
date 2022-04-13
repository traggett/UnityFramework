using UnityEngine;
using System;

namespace Framework
{
	using DynamicValueSystem;
	using Graphics;

	namespace NodeGraphSystem
	{
		[NodeCategory("Colors")]
		[Serializable]
		public class ColorLightnessNode : Node, IValueSource<float>
		{
			#region Public Data	
			public NodeInputField<Color> _value = Color.clear;
			#endregion

			#region Private Data
			private float _lightness;
			#endregion

			#region Node
			public override void Update(float time, float deltaTime)
			{
				_lightness = HSLColor.FromRGBA(_value).l;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return ColorNodes.kNodeColor;
			}
#endif
			#endregion

			#region IValueSource<float>
			public float GetValue()
			{
				return _lightness;
			}
			#endregion
		}
	}
}