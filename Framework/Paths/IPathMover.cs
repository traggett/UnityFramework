using UnityEngine;
using Framework.Maths;

namespace Framework
{
	namespace Paths
	{
		public interface IPathMover
		{
			PathPosition GetCurrentPosition();
			void SetCurrentPosition(PathPosition pathPosition);
			bool MoveTo(PathPosition targetPosition, IPathMovementStyle movementStyle);
		}
	}
}