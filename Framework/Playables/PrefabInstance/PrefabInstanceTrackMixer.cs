using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public class PrefabInstanceTrackMixer : ParentBindingTrackMixer
		{
			private GameObject _prefabInstance;

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				PrefabInstanceTrack track = (PrefabInstanceTrack)_trackAsset;
				TimelineClip clip = track.GetMasterClip();

				if (clip != null)
				{
					double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
					double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

					if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
					{
						CreatePrefabInstance();
						SetChildTrackBindings(_prefabInstance);
					}
					else
					{
						DestroyPrefabInstance();
						ClearChildTrackBindings();
					}
				}
			}

			public override void OnPlayableDestroy(Playable playable)
			{
				base.OnPlayableDestroy(playable);
				DestroyPrefabInstance();
			}

			private void CreatePrefabInstance()
			{
				if (_prefabInstance == null)
				{
					DestroyPrefabInstance();
					PrefabInstanceTrack track = (PrefabInstanceTrack)_trackAsset;
					_prefabInstance = track._prefab.LoadAndInstantiatePrefab();
				}
			}

			private void DestroyPrefabInstance()
			{
				if (_prefabInstance != null)
				{
					if (Application.isPlaying)
						Object.Destroy(_prefabInstance);
					else
						Object.DestroyImmediate(_prefabInstance);

					_prefabInstance = null;
				}
			}

		}
	}
}
