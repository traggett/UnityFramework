using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		public class MaterialColorParamPlayableBehaviour : PlayableBehaviour
		{
			[NonSerialized]
			public PlayableAsset _clip;

			public Color _value;
		}
	}
}
