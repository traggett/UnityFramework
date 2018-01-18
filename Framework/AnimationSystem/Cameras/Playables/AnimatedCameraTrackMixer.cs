using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Maths;

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

				int inputCount = playable.GetInputCount();
				AnimatedCameraState blendedState = _defaultState;
				
				for (int i = 0; i < inputCount; i++)
				{
					float inputWeight = playable.GetInputWeight(i);

					if (!Mathf.Approximately(inputWeight, 0f))
					{
						ScriptPlayable<AnimatedCameraPlayableBehaviour> inputPlayable = (ScriptPlayable<AnimatedCameraPlayableBehaviour>)playable.GetInput(i);
						AnimatedCameraPlayableBehaviour input = inputPlayable.GetBehaviour();

						if (input._snapshot != null)
						{
							AnimatedCameraState behaviourState = input._snapshot.GetState();

							if (input._snapshotTo != null)
							{
								float normalisedTime = (float)(inputPlayable.GetTime() * input.inverseDuration);
								behaviourState = behaviourState.InterpolateTo(_trackBinding, input._snapshotTo.GetState(), input._easeType, normalisedTime);
							}

							//Fade playable over current state using its weight
							blendedState = blendedState.InterpolateTo(_trackBinding, behaviourState, eInterpolation.Linear, inputWeight);
						}
					}
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
