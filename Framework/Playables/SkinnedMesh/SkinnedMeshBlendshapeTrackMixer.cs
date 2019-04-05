using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public class SkinnedMeshBlendshapeTrackMixer : PlayableBehaviour, ITrackMixer, IParentBindableTrackMixer
		{
			private TrackAsset _trackAsset;
			private PlayableDirector _director;

			protected SkinnedMeshRenderer _trackBinding;
			
			private bool _parentBinding;
			private bool _firstFrame = true;

			private float _defaultValue;
			
			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				SkinnedMeshBlendshapeTrack track = (SkinnedMeshBlendshapeTrack)_trackAsset;

				if (!_parentBinding)
					_trackBinding = playerData as SkinnedMeshRenderer;

				if (_trackBinding == null || track._blendShapeIndex >= _trackBinding.sharedMesh.blendShapeCount)
					return;

				if (_firstFrame)
				{
					_defaultValue = _trackBinding.GetBlendShapeWeight(track._blendShapeIndex);
					_firstFrame = false;
				}

				float weight = _defaultValue;

				int numInputs = playable.GetInputCount();

				for (int i = 0; i < numInputs; i++)
				{
					float inputWeight = playable.GetInputWeight(i);

					if (inputWeight > 0.0f)
					{
						ScriptPlayable<SkinnedMeshBlendshapePlayableBehaviour> scriptPlayable = (ScriptPlayable<SkinnedMeshBlendshapePlayableBehaviour>)playable;
						SkinnedMeshBlendshapePlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

						weight = Mathf.Lerp(weight, inputBehaviour._weight, inputWeight);
					}
				}

				_trackBinding.SetBlendShapeWeight(track._blendShapeIndex, weight);
			}

			public override void OnPlayableDestroy(Playable playable)
			{
				_firstFrame = true;

				if (_trackBinding == null)
					return;
			}

			#region ITrackMixer
			public void SetTrackAsset(TrackAsset trackAsset, PlayableDirector playableDirector)
			{
				_trackAsset = trackAsset;
				_director = playableDirector;
			}

			public TrackAsset GetTrackAsset()
			{
				return _trackAsset;
			}
			#endregion

			#region IParentBindable
			public void SetParentBinding(object playerData)
			{
				if (playerData is GameObject)
				{
					_trackBinding = SkinnedMeshBlendshapeTrack.GetSkinnedMeshFromGameObject((GameObject)playerData);
					_parentBinding = true;
				}
				else if (playerData is Transform)
				{
					_trackBinding = SkinnedMeshBlendshapeTrack.GetSkinnedMeshFromGameObject(((Transform)playerData).gameObject);
					_parentBinding = true;
				}
				else
				{
					_parentBinding = false;
				}
			}

			public void ClearParentBinding()
			{
				_trackBinding = null;
				_parentBinding = false;
			}
			#endregion
		}
	}
}
