using UnityEngine;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		public class Vector2NodeGraphComponent : NodeGraphComponent, IValueSource<Vector2>
		{
			#region IValueSource<Vector2>
			public Vector2 GetValue()
			{
				if (_nodegraph != null)
					return _nodegraph.GetValue<Vector2>();

				return Vector2.zero;
			}
			#endregion
		}
	}
}