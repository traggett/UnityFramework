using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		[Serializable]
		[NotKeyable]
		public class ParentBindingMasterClip : PlayableAsset, ITimelineClipAsset
		{
			public ClipCaps clipCaps
			{
				get { return ClipCaps.Extrapolation | ClipCaps.Looping; }
			}

			public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
			{
				ScriptPlayable<ParentBindingBehaviour> playable = ScriptPlayable<ParentBindingBehaviour>.Create(graph, new ParentBindingBehaviour());
				return playable;
			}
		}
}
}
