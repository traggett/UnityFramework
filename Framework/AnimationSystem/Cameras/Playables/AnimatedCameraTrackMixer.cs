using System.Collections.Generic;

using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Maths;
	using System;

	namespace AnimationSystem
	{
		public class AnimatedCameraTrackMixer : PlayableBehaviour
		{
			private PlayableDirector _director;
			private IEnumerable<TimelineClip> _clips;
			private AnimatedCameraState _defaultState;
			private AnimatedCamera _trackBinding;
			private bool _firstFrameHappened;
			
			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				_trackBinding = playerData as AnimatedCamera;

				if (_trackBinding == null)
					return;

				if (!_firstFrameHappened)
				{
					_defaultState = _trackBinding.GetState();
					_firstFrameHappened = true;
				}

				AnimatedCameraState blendedState = _defaultState;

				int inputPort = 0;
				foreach (TimelineClip clip in _clips)
				{
					ScriptPlayable<AnimatedCameraPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatedCameraPlayableBehaviour>)playable.GetInput(inputPort);
					AnimatedCameraPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

					if (inputBehaviour != null)
					{
						double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
						double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

						//If currently playing
						if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
						{
							float inputWeight = playable.GetInputWeight(inputPort);

							AnimatedCameraState behaviourState = inputBehaviour._snapshot.GetState();

							//Interpolated behavior
							if (inputBehaviour._snapshotTo != null)
							{
								double clipT = Math.Min(Math.Max(_director.time - clip.start, 0.0) / clip.duration, 1.0);
								behaviourState = behaviourState.InterpolateTo(_trackBinding, inputBehaviour._snapshotTo.GetState(), inputBehaviour._easeType, (float)clipT);
							}

							//Fade playable over current state using its weight
							blendedState = blendedState.InterpolateTo(_trackBinding, behaviourState, eInterpolation.Linear, inputWeight);
						}
					}

					++inputPort;
				}

				_trackBinding.SetState(blendedState);
			}

			public override void OnGraphStop(Playable playable)
			{
				if (_trackBinding != null)
					_trackBinding.SetState(_defaultState);
				
				_firstFrameHappened = false;
			}

			public void SetClips(PlayableDirector director, IEnumerable<TimelineClip> clips)
			{
				_director = director;
				_clips = clips;
			}
		}
	}
}
