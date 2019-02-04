using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public abstract class AnimatorParamTrack : BaseTrackAsset, IParentBindableTrack
		{
			public string _parameterId;
		}
	}
}