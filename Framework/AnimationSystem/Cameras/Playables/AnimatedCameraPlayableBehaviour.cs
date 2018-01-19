using System;
using UnityEngine.Playables;

namespace Framework
{
	using Maths;
	
	namespace AnimationSystem
	{
		[Serializable]
		public class AnimatedCameraPlayableBehaviour : PlayableBehaviour
		{
			public IAnimatedCameraStateSource _snapshot;
			public IAnimatedCameraStateSource _snapshotTo;
			public eInterpolation _easeType;
		}
	}
}
