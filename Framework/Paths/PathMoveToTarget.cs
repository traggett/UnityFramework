using Framework.Maths;

namespace Framework
{
	namespace Paths
	{
		[System.Serializable]
		public class PathMoveToTarget
		{
			public PathPosition _position;
			public float _speed;
			public bool _moveThrough;
			public Direction1D _finalFaceDirection;
		}
	}
}