using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace AnimationSystem
	{
		public class LegacyAnimatorPlayableBehaviour : PlayableBehaviour
		{
			public PlayableAsset _clipAsset;
			public AnimationClip _animation;
			public float _animationSpeed;
		}
	}
}
