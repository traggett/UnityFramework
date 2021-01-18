using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		public class LegacyAnimatorChannelTrackMixer : PlayableBehaviour, ITrackMixer
		{
			protected TrackAsset _trackAsset;
			protected PlayableDirector _director;
			private LegacyAnimator.AnimationParams _primaryAnimation;
			private LegacyAnimator.AnimationParams[] _backgroundAnimations;

			public override void PrepareFrame(Playable playable, FrameData info)
			{
				int numInputs = playable.GetInputCount();

				_primaryAnimation = new LegacyAnimator.AnimationParams();
				List<LegacyAnimator.AnimationParams> backgroundAnimations = new List<LegacyAnimator.AnimationParams>();
				
				for (int i = 0; i < numInputs; i++)
				{
					ScriptPlayable<LegacyAnimatorPlayableBehaviour> scriptPlayable = (ScriptPlayable<LegacyAnimatorPlayableBehaviour>)playable.GetInput(i);
					LegacyAnimatorPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

					if (inputBehaviour != null && inputBehaviour._animation != null)
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
									bool isPrimaryClip = IsPrimaryClip(clip);

									//Work out track time
									float animationDuration = inputBehaviour._animation.length;
									float trackTime = GetExtrapolatedTrackTime(clip, _director.time, animationDuration);

									if (isPrimaryClip)
									{
										_primaryAnimation._animName = inputBehaviour._animation.name;
										_primaryAnimation._time = trackTime;
										_primaryAnimation._weight = inputWeight;
										_primaryAnimation._speed = inputBehaviour._animationSpeed;
									}
									else
									{
										LegacyAnimator.AnimationParams backroundAnimation = new LegacyAnimator.AnimationParams
										{
											_animName = inputBehaviour._animation.name,
											_time = trackTime,
											_weight = 1.0f,
											_speed = inputBehaviour._animationSpeed,
										};
										backgroundAnimations.Add(backroundAnimation);
									}
								}
							}
						}
					}
				}

				_backgroundAnimations = backgroundAnimations.ToArray();
			}

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				
			}

			public void GetData(out int channel, out LegacyAnimator.AnimationParams primaryAnimation, out LegacyAnimator.AnimationParams[] backgroundAnimations)
			{
				channel = ((LegacyAnimatorChannelTrack)_trackAsset)._animationChannel;
				primaryAnimation = _primaryAnimation;
				backgroundAnimations = _backgroundAnimations;
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


			protected bool IsPrimaryClip(TimelineClip clip)
			{
				//If doing pre extrapolation then is primary
				if (clip.hasPreExtrapolation && clip.extrapolatedStart <= _director.time && _director.time <= clip.start)
					return true;

				//If doing post extrapolation then is primary
				if (clip.hasPostExtrapolation && clip.start <= _director.time && _director.time <= clip.start + clip.extrapolatedDuration)
					return true;

				//if this clip is blending in, this is primary
				if (clip.hasBlendIn && clip.start <= _director.time && _director.time <= clip.start + clip.blendInDuration)
					return true;

				//if this clip is blending out, is not primary
				if (clip.hasBlendOut && clip.end - clip.blendOutDuration <= _director.time && _director.time <= clip.end)
					return false;

				//if during clip main then is primary
				if (clip.start <= _director.time && _director.time <= clip.end)
					return true;

				return false;
			}

			protected static float GetExtrapolatedTrackTime(TimelineClip clip, double directorTime, float animationLength)
			{
				TimelineClip.ClipExtrapolation extrapolation = directorTime < clip.start ? clip.preExtrapolationMode : clip.postExtrapolationMode;
				float time = (float)(directorTime - clip.start);

				if (clip.start <= directorTime && directorTime < clip.end)
					return time;

				if (animationLength <= 0.0f)
					return 0.0f;

				switch (extrapolation)
				{
					case TimelineClip.ClipExtrapolation.Continue:
					case TimelineClip.ClipExtrapolation.Hold:
						return time < 0.0f ? 0.0f : (float)clip.end;
					case TimelineClip.ClipExtrapolation.Loop:
						{
							if (time < 0.0f)
							{
								float t = -time / animationLength;
								int n = Mathf.FloorToInt(t);
								float fraction = animationLength - (t - n);

								time = (animationLength * n) + fraction;
							}

							return time;
						}
					case TimelineClip.ClipExtrapolation.PingPong:
						{
							float t = Mathf.Abs(time) / animationLength;
							int n = Mathf.FloorToInt(t);
							float fraction = t - n;

							if (n % 2 == 1)
								fraction = animationLength - fraction;

							return (animationLength * n) + fraction;
						}
					case TimelineClip.ClipExtrapolation.None:
					default:
						return 0.0f;
				}
			}
		}
	}
}