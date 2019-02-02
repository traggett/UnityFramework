using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class AnimatorPlayableBehaviour : PlayableBehaviour
		{
			[HideInInspector]
			public PlayableAsset _clipAsset;

			[HideInInspector]
			public float _test;

			[Serializable]
			public struct FloatParam
			{
				public string _id;
				public float _value;
			}

			public FloatParam[] _floatParams;
		}
	}
}
