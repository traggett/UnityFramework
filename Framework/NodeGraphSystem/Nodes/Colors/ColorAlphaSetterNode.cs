using UnityEngine;
using System;

namespace Framework
{
	using ValueSourceSystem;
	
	namespace NodeGraphSystem
	{
		[NodeCategory("Colors")]
		[Serializable]
		public class ColorAlphaSetterNode : Node, IValueSource<Color>
		{
			#region Public Data	
			public NodeInputField<Color> _value = Color.clear;
			public NodeInputField<float> _alpha = 0.0f;
			#endregion

			#region IValueSource<float>
			public Color GetValue()
			{
				Color color = _value;
				color.a = _alpha;
				return color;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return ColorNodes.kNodeColor;
			}
#endif
			#endregion
		}
	}
}