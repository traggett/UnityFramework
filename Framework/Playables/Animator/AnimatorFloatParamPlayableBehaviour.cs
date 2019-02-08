using System;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorFloatParamPlayableBehaviour : PlayableBehaviour
		{
			[NonSerialized]
			public PlayableAsset _clip;

			public float _value;
		}
	}
}
