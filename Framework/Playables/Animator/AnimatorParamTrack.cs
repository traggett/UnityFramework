using UnityEngine;

namespace Framework
{
	namespace Playables
	{
		public abstract class AnimatorParamTrack : BaseTrackAsset, IParentBindableTrack
		{
			public string _parameterId;

			public static Animator GetAnimatorFromGameObject(GameObject gameObject)
			{
				Animator animator = gameObject.GetComponent<Animator>();

				if (animator == null)
					animator = gameObject.GetComponentInChildren<Animator>();

				return animator;
			}
		}
	}
}