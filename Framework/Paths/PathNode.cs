using UnityEngine;
using Framework.Utils;

namespace Framework
{
	namespace Paths
	{
		public class PathNode : MonoBehaviour
		{
			private Path[] _parentPaths;

			#region IPathNode 
			public Path[] GetPaths()
			{
				return _parentPaths;
			}
			#endregion

			public void AddParent(Path spline)
			{
				ArrayUtils.Add(ref _parentPaths, spline);
			}

			public void ClearParent(Path spline)
			{
				if (spline != null && _parentPaths != null)
					ArrayUtils.Remove(ref _parentPaths, spline);
			}

#if UNITY_EDITOR
			void OnDrawGizmosSelected()
			{
				if (GetPaths() != null)
				{
					foreach (Path path in GetPaths())
					{
						if (path != null)
						{
							path.DebugDraw();
						}
					}
				}
			}
#endif
		}
	}
}