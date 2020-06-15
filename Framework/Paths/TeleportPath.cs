using UnityEngine;

namespace Framework
{
	namespace Paths
	{
		public class TeleportPath : Path
		{	
			#region IPath 
			public override float GetDistanceBetween(float fromT, float toT)
			{
				//Path has no length
				return 0.0f;
			}

			protected override PathPosition GetPointAt(float pathT)
			{
				return new PathPosition(this, 0f, GetNodePosition(0), Vector3.forward, Vector3.up, _nodes[0]._width);
			}

			public override PathPosition TravelPath(float fromT, float toT, float distance)
			{
				return GetPoint(0.0f);
			}

			public override PathPosition GetClosestPoint(Vector3 position, out float distanceSqr)
			{
				PathPosition pathPos = GetPoint(0.0f);
				distanceSqr = (position - pathPos._pathPosition).sqrMagnitude;
				return pathPos;
			}

			public override PathPosition GetClosestPoint(Ray ray, out float distanceSqr)
			{
				PathPosition pathPos = GetPoint(0.0f);
				//TO DO! Should return distance between closest point on ray and position
				distanceSqr = (ray.origin - pathPos._pathPosition).sqrMagnitude;
				return pathPos;
			}
			#endregion
		}
	}
}