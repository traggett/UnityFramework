using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class ParticleSystemPlayableBehaviour : PlayableBehaviour
		{
			[NonSerialized]
			public PlayableAsset _clipAsset;

			[Range(0f, 1f)]
			public float _emissionRate;
		}
	}
}
