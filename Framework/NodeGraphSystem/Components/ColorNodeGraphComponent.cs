using UnityEngine;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		public class ColorNodeGraphComponent : NodeGraphComponent, IValueSource<Color>
		{
			#region IValueSource<Color>
			public Color GetValue()
			{
				if (_nodegraph != null)
					return _nodegraph.GetValue<Color>();

				return Color.black;
			}
			#endregion
		}
	}
}