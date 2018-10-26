using UnityEngine;
using System;

namespace Framework
{
	namespace AnimationSystem
	{
		[Serializable]
		public struct IAnimatorRefProperty 
		{
			[SerializeField]
			private Component _animator;

#if UNITY_EDITOR
			[SerializeField]
			private float _editorHeight;
#endif

			public static implicit operator Component(IAnimatorRefProperty value)
			{
				return value._animator;
			}

			public IAnimator GetAnimator()
			{
				return (IAnimator)_animator;
			}
		}
	}
}