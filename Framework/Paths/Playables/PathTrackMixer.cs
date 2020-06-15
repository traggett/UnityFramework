using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables;
	using UnityEngine;

	namespace Paths
	{
		public class PathTrackMixer : PlayableBehaviour, ITrackMixer
		{
			protected TrackAsset _trackAsset;
			protected PlayableDirector _director;
			protected ITrackMixer _parentMixer;
			private Transform _trackBinding;
			private bool _firstFrame = true;
			private Vector3 _defaultPosition;
			private Quaternion _defaultRotation;

			public void Init(ITrackMixer parentMixer)
			{
				_parentMixer = parentMixer;
			}

			public override void PrepareFrame(Playable playable, FrameData info)
			{

			}

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				_trackBinding = playerData as Transform;

				if (_trackBinding == null)
					return;

				if (_firstFrame)
				{
					_defaultPosition = _trackBinding.position;
					_defaultRotation = _trackBinding.rotation;
					_firstFrame = false;
				}

				Vector3 position = _defaultPosition;
				Quaternion rotation  = _defaultRotation;

				int numInputs = playable.GetInputCount();
				
				for (int i = 0; i < numInputs; i++)
				{
					ScriptPlayable<PathPlayableBehaviour> scriptPlayable = (ScriptPlayable<PathPlayableBehaviour>)playable.GetInput(i);
					PathPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

					if (inputBehaviour != null && inputBehaviour._path != null)
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
									//To do handle loops etc

									double t = Mathf.Clamp01((float)(_director.time - clip.start) / (float)clip.duration);
									PathPosition pos = inputBehaviour._path.GetPoint((float)t);

									position = Vector3.Lerp(position, pos._pathPosition, inputWeight);
									rotation = Quaternion.Slerp(rotation, Quaternion.LookRotation(pos._pathForward, pos._pathUp), inputWeight);
								}
							}
						}
					}
				}

				_trackBinding.position = position;
				_trackBinding.rotation = rotation;
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