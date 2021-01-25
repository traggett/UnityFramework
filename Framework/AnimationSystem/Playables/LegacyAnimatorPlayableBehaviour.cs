using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace AnimationSystem
	{
		public class LegacyAnimatorPlayableBehaviour : PlayableBehaviour
		{
			public LegacyAnimationClipAsset _clipAsset;
			public AnimationClip _animation;
		}
	}
}
