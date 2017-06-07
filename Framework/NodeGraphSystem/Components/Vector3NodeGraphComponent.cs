using UnityEngine;

namespace Framework
{
	using ValueSourceSystem;

	namespace NodeGraphSystem
	{
		public class Vector3NodeGraphComponent : NodeGraphComponent, IValueSource<Vector3>
		{
			#region IValueSource<Vector3>
			public Vector3 GetValue()
			{
				if (_nodegraph != null)
					return _nodegraph.GetValue<Vector3>();

				return Vector3.zero;
			}
			#endregion
		}
	}
}