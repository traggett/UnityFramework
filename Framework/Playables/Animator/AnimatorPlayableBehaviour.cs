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
			public float _test;
			public float[] _tests = new float[0];

			[HideInInspector]
			public PlayableAsset _clipAsset;

			[Serializable]
			public class FloatParam
			{
				public string _id;
				public float _value;
			}
			public FloatParam[] _floatParams = new FloatParam[0];

			[Serializable]
			public class IntParam
			{
				public string _id;
				public int _value;
			}
			public IntParam[] _intParams = new IntParam[0];

			[Serializable]
			public class BoolParam
			{
				public string _id;
				public bool _value;
			}
			public BoolParam[] _boolParams = new BoolParam[0];
		}
	}
}
