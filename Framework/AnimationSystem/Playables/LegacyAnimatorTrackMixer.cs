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

			private class ChannelData
			{
				public int _channel;
				public LegacyAnimator.AnimationParams _primaryAnimation;
				public LegacyAnimator.AnimationParams[] _backgroundAnimations;
			}

			private List<ChannelData> _channelData = new List<ChannelData>();

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

			public LegacyAnimator GetTrackBinding()
			{
				return _trackBinding;
			}

			public override void PrepareFrame(Playable playable, FrameData info)
			{
				_channelData.Clear();

#if UNITY_EDITOR
				if (_trackAsset != null)
					_trackAsset.EnsureMasterClipExists();
#endif
			}

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				int inputs = playable.GetInputCount();

				for (int i=0; i<inputs; i++)
				{
					float inputWeight = playable.GetInputWeight(i);

					if (inputWeight > 0f && TimelineUtils.IsScriptPlayable(playable.GetInput(i), out LegacyAnimatorChannelTrackMixer channelMixer))
					{
						channelMixer.GetData(out int channel, out LegacyAnimator.AnimationParams primaryAnimation, out LegacyAnimator.AnimationParams[] backgroundAnimations);
						_trackBinding.SetChannelData(channel, primaryAnimation, backgroundAnimations);

#if UNITY_EDITOR
						//For previewing in editor can't blend between clips
						if (!Application.isPlaying)
						{
							AnimationClip clip = _trackBinding.GetClip(primaryAnimation._animName);
							if (clip != null)
							{
								AnimationMode.BeginSampling();
								AnimationMode.SampleAnimationClip(_trackBinding.gameObject, clip, primaryAnimation._time);
								AnimationMode.EndSampling();
							}					
						}
#endif
					}
				}

				
				


				//TO DO! Need to also preview this in editor somehow???
				//How does tha nimation window work
			}

			public override void OnGraphStop(Playable playable)
			{

			}
		}
	}
}