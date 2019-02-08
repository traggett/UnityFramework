using System;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorBoolParamPlayableBehaviour : PlayableBehaviour
		{
			[NonSerialized]
			public PlayableAsset _clip;

			public bool _value;
		}
	}
}
