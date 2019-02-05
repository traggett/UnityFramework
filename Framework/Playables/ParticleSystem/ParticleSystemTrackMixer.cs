using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public class ParticleSystemTrackMixer : PlayableBehaviour, ITrackMixer
		{
			private TrackAsset _trackAsset;
			private PlayableDirector _director;

			private ParticleSystem _trackBinding;
			private bool _parentBinding;


			private struct ObjectData
			{
				public float _rateOverTimeMultiplier;
				public float _rateOverDistanceMultiplier;
			}
				
			private bool _firstFrame = true;
			private ObjectData _defaultData;

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				if (!_parentBinding)
					_trackBinding = playerData as ParticleSystem;

				if (_trackBinding == null)
					return;

				if (_firstFrame)
				{
					_firstFrame = false;
					_defaultData._rateOverTimeMultiplier = _trackBinding.emission.rateOverTimeMultiplier;
					_defaultData._rateOverDistanceMultiplier = _trackBinding.emission.rateOverDistanceMultiplier;
				}

				int numInputs = playable.GetInputCount();

				ObjectData data = _defaultData;

				for (int i = 0; i < numInputs; i++)
				{
					ScriptPlayable<ParticleSystemPlayableBehaviour> scriptPlayable = (ScriptPlayable<ParticleSystemPlayableBehaviour>)playable.GetInput(i);
					ParticleSystemPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

					if (inputBehaviour != null)
					{
						float inputWeight = playable.GetInputWeight(i);

						if (inputWeight > 0.0f)
						{
							TimelineClip clip = TimelineUtils.GetClip(_trackAsset, inputBehaviour._Clip);

							if (clip != null)
							{
								double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
								double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

								if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
								{
									data._rateOverTimeMultiplier = inputBehaviour._emissionRate;
									data._rateOverDistanceMultiplier = inputBehaviour._emissionRate;
								}
							}
						}
					}
				}

				SetData(_trackBinding, data);
			}

			public override void OnPlayableDestroy(Playable playable)
			{
				_firstFrame = true;

				if (_trackBinding == null)
					return;


				SetData(_trackBinding, _defaultData);
			}

			private static void SetData(ParticleSystem particleSystem, ObjectData data)
			{
				ParticleSystem.EmissionModule emission = particleSystem.emission;
				emission.rateOverTimeMultiplier = data._rateOverTimeMultiplier;
				emission.rateOverDistanceMultiplier = data._rateOverDistanceMultiplier;
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
		}
	}
}