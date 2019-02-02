using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public class AnimatorParamTrackMixer : PlayableBehaviour, ITrackMixer, IParentBindableTrackMixer
		{
			private TrackAsset _trackAsset;
			private PlayableDirector _director;

			private Animator _trackBinding;
			private bool _parentBinding;
			private bool _firstFrame = true;

			private Vector3 _defaultPosition;
			private Quaternion _defalultRotation;

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				if (!_parentBinding)
					_trackBinding = playerData as Animator;

				if (_trackBinding == null)
					return;

				if (_firstFrame)
				{
					
					_firstFrame = false;
				}

				//_trackBinding.position = _defaultPosition;
				//_trackBinding.rotation = _defalultRotation;

				int numInputs = playable.GetInputCount();

				for (int i = 0; i < numInputs; i++)
				{
					ScriptPlayable<AnimatorPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorPlayableBehaviour>)playable.GetInput(i);
					AnimatorPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

					if (inputBehaviour != null)
					{
						float inputWeight = playable.GetInputWeight(i);

						if (inputWeight > 0.0f)
						{
							TimelineClip clip = TimelineUtils.GetClip(_trackAsset, inputBehaviour._clipAsset);

							if (clip != null)
							{
								double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
								double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

								if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
								{
									//float distance = inputBehaviour._splineProgress * inputBehaviour._spline.PathLength;
									//float pathT = inputBehaviour._spline.GetPathFractionAtDistance(distance);

									//Vector3 position = inputBehaviour._spline.EvaluatePosition(pathT);
									//Quaternion rotation = inputBehaviour._spline.EvaluateOrientation(pathT);

									//_trackBinding.position = position;
									//_trackBinding.rotation = rotation;
								}
							}
						}
					}
				}
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
			public void SetBinding(object playerData)
			{
				if (playerData is GameObject)
				{
					_trackBinding = ((GameObject)playerData).GetComponent<Animator>();
					_parentBinding = true;
				}
				else if (playerData is Transform)
				{
					_trackBinding = ((Transform)playerData).gameObject.GetComponent<Animator>();
					_parentBinding = true;
				}
				else
				{
					_parentBinding = false;
				}
			}
			#endregion
		}
	}
}
