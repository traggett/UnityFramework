using System.Collections.Generic;
using Framework.Playables;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	namespace AnimationSystem
	{
		public class LegacyAnimatorTrackMixer : PlayableBehaviour, ITrackMixer
		{
			private LegacyAnimatorTrack _trackAsset;
			private PlayableDirector _director;
			private LegacyAnimator _trackBinding;

			private struct ChannelData
			{
				public int _channel;
				public LegacyAnimator.AnimationParams _primaryAnimation;
				public List<LegacyAnimator.AnimationParams> _backgroundAnimations;
			}
			private ChannelData[] _channelData;

			#region ITrackMixer
			public void SetTrackAsset(TrackAsset trackAsset, PlayableDirector playableDirector)
			{
				_trackAsset = trackAsset as LegacyAnimatorTrack;
				_director = playableDirector;
				_trackBinding = _director.GetGenericBinding(GetTrackAsset()) as LegacyAnimator;
			}
			
			public TrackAsset GetTrackAsset()
			{
				return _trackAsset;
			}
			#endregion

			public void SetChannels(int[] channels)
			{
				_channelData = new ChannelData[channels.Length];

				for (int i=0; i<channels.Length; i++)
				{
					_channelData[i] = new ChannelData
					{
						_channel = channels[i],
						_backgroundAnimations = new List<LegacyAnimator.AnimationParams>()
					};
				}
			}

			public LegacyAnimator GetTrackBinding()
			{
				return _trackBinding;
			}

			public override void PrepareFrame(Playable playable, FrameData info)
			{
				ClearChannelData();

				int numInputs = playable.GetInputCount();

				for (int i = 0; i < numInputs; i++)
				{
					float inputWeight = playable.GetInputWeight(i);

					if (inputWeight > 0f && TimelineUtils.IsScriptPlayable(playable.GetInput(i), out LegacyAnimatorPlayableBehaviour inputBehaviour))
					{
						TimelineClip clip = inputBehaviour._clipAsset.GetTimelineClip();

						if (clip != null)
						{
							double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
							double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

							if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
							{
								int ch = GetChannelIndex(inputBehaviour._clipAsset.GetChannel());

								bool isPrimaryClip = TimelineUtils.IsPrimaryClip(clip, _director);
								float trackTime = TimelineUtils.GetExtrapolatedTrackTime(clip, _director.time, inputBehaviour._animation.length);

								if (isPrimaryClip)
								{
									_channelData[ch]._primaryAnimation._animName = inputBehaviour._animation.name;
									_channelData[ch]._primaryAnimation._time = trackTime;
									_channelData[ch]._primaryAnimation._weight = inputWeight;
									_channelData[ch]._primaryAnimation._speed = inputBehaviour._animationSpeed;
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
									_channelData[ch]._backgroundAnimations.Add(backroundAnimation);
								}
							}
						}
					}
				}
			}

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				for (int i=0; i<_channelData.Length; i++)
				{
					ApplyAnimations(ref _channelData[i]);
				}
			}

			public override void OnGraphStop(Playable playable)
			{

			}

			private void ApplyAnimations(ref ChannelData channel)
			{
				if (Application.isPlaying)
				{
					_trackBinding.SetChannelData(channel._channel, channel._primaryAnimation, channel._backgroundAnimations.ToArray());
				}
#if UNITY_EDITOR
				//For previewing in editor can't blend between clips - the primary clip will play at full weight
				else
				{
					AnimationClip clip = _trackBinding.GetClip( channel._primaryAnimation._animName);
					
					if (clip != null)
					{
						AnimationMode.BeginSampling();
						AnimationMode.SampleAnimationClip(_trackBinding.gameObject, clip, channel._primaryAnimation._time);
						AnimationMode.EndSampling();
					}					
				}
#endif
			}

			private void ClearChannelData()
			{
				for (int i=0; i<_channelData.Length; i++)
				{
					_channelData[i]._primaryAnimation = default;
					_channelData[i]._backgroundAnimations.Clear();
				}
			}

			private int GetChannelIndex(int channel)
			{
				for (int i=0; i<_channelData.Length; i++)
				{
					if (_channelData[i]._channel == channel)
						return i;
				}

				return -1;
			}
		}
	}
}