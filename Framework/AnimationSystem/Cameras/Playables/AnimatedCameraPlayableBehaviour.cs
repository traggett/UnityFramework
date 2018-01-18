using System;
using UnityEngine.Playables;

namespace Framework
{
	using Maths;
	using UnityEngine;

	namespace AnimationSystem
	{
		[Serializable]
		public class AnimatedCameraPlayableBehaviour : PlayableBehaviour
		{
			public IAnimatedCameraStateSource _snapshot;
			public IAnimatedCameraStateSource _snapshotTo;
			public eInterpolation _easeType;
			public float inverseDuration;

			public override void OnGraphStart(Playable playable)
			{
				double duration = playable.GetDuration();
				if (Mathf.Approximately((float)duration, 0f))
					throw new UnityException("A AnimatedCameraTween cannot have a duration of zero.");

				inverseDuration = 1f / (float)duration;
			}
		}
	}
}
