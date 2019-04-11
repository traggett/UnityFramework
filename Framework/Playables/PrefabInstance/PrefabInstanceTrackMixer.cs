using Framework.Utils;
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
			private Transform _prefabSpawnPoint;
			private bool _prefabSpawnPointIsParent;

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
					else if (_prefabInstance != null)
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

			public void SetSpawnPoint(Transform transform, bool setAsParent)
			{
				_prefabSpawnPoint = transform;
				_prefabSpawnPointIsParent = setAsParent;
			}

			private void CreatePrefabInstance()
			{
				if (_prefabInstance == null)
				{
					DestroyPrefabInstance();
					PrefabInstanceTrack track = (PrefabInstanceTrack)_trackAsset;
					_prefabInstance = track._prefab.LoadAndInstantiatePrefab(_prefabSpawnPoint != null ?_prefabSpawnPoint.parent : null);

					if (_prefabSpawnPoint != null)
					{
						if (_prefabSpawnPointIsParent)
						{
							_prefabInstance.transform.parent = _prefabSpawnPoint;
							_prefabInstance.transform.localPosition = Vector3.zero;
							_prefabInstance.transform.localRotation = Quaternion.identity;
							_prefabInstance.transform.localScale = Vector3.one;
						}
						else
						{
							_prefabInstance.transform.position = _prefabSpawnPoint.position;
							_prefabInstance.transform.rotation = _prefabSpawnPoint.rotation;
							_prefabInstance.transform.localScale = _prefabSpawnPoint.localScale;
						}
					}
				}
			}

			private void DestroyPrefabInstance()
			{
				if (_prefabInstance != null)
				{
					GameObjectUtils.SafeDestroy(_prefabInstance);
					_prefabInstance = null;
				}
			}

		}
	}
}
