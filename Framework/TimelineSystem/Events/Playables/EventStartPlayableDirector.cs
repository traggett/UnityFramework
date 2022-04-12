using UnityEngine;
using UnityEngine.Playables;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Playables")]
		public class EventStartPlayableDirector : Event
		{
			#region Public Data
			public ComponentRef<PlayableDirector> _director;
			public AssetRef<PlayableAsset> _playableAsset;
			public DirectorWrapMode _wrapMode;
			#endregion

			#region Event
			public override void Trigger()
			{
				PlayableDirector director = _director.GetComponent();

				if (director != null)
				{
					if (_playableAsset.IsValid())
					{
						PlayableAsset playable = _playableAsset.LoadAsset();
						director.Play(playable, _wrapMode);
					}
					else
					{
						director.Play(director.playableAsset, _wrapMode);
					}
				}
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(0.859f, 0.439f, 0.576f);
			}

			public override string GetEditorDescription()
			{
				return "Play (<b>"+ _director + "</b>)";
			}
#endif
			#endregion
		}
	}
}
