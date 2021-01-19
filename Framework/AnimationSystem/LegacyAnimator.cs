using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		[RequireComponent(typeof(Animation))]
		public class LegacyAnimator : MonoBehaviour, IAnimator
		{
			#region Public Data
			public struct AnimationParams
			{
				public string _animName;
				public float _time;
				public float _speed;
				public float _weight;
			}
			#endregion

			#region Private Data
			//The number of animation that can be blending at any one time per each channel
			private static readonly int kNumberOfBackgroundLayers = 2;

			private Animation _animation;
			private Animation AnimationComponent
			{
				get
				{
					if (_animation == null)
					{
						_animation = GetComponent<Animation>();
					}

					return _animation;
				}
			}
			private struct ChannelLayer
			{
				//Layer in animation component
				public readonly int _layer;
				//The current animation
				public AnimationState _animation;
				//Used for blending when stopping
				public float _origWeight;		
				
				public ChannelLayer(int layer)
				{
					_layer = layer;
					_animation = null;
					_origWeight = 1f;
				}
			}

			private class ChannelGroup
			{
				public enum State
				{
					Stopped,
					Playing,
					BlendingIn,
					Stopping,
				}
				public State _state = State.Stopped;

				public readonly int _channel;
				public ChannelLayer _primaryLayer;
				public ChannelLayer[] _backgroundLayers;

				//Primary track blending data
				public float _lerpT;
				public float _lerpSpeed;
				public InterpolationType _lerpEase;
				public float _targetWeight;

				//Queued animation data
				public string _queuedAnimation;
				public float _queuedAnimationWeight;
				public float _queuedAnimationBlendTime;
				public WrapMode _queuedAnimationWrapMode;
				public InterpolationType _queuedAnimationEase;

				public ChannelGroup(int channel)
				{
					_channel = channel;
					_primaryLayer = new ChannelLayer((channel * (kNumberOfBackgroundLayers + 1)) + kNumberOfBackgroundLayers);
					_backgroundLayers = new ChannelLayer[kNumberOfBackgroundLayers];
					for (int i = 0; i<kNumberOfBackgroundLayers; i++)
					{
						_backgroundLayers[i] = new ChannelLayer(_primaryLayer._layer - i - 1);
					}
				}
			}
			private readonly List<ChannelGroup> _channels = new List<ChannelGroup>();
			#endregion

			#region MonoBehaviour Calls
			void Update()
			{
				foreach (ChannelGroup channelGroup in _channels)
				{
					UpdateChannelBlends(channelGroup);
				}
			}
			#endregion

			#region IAnimator
			public void Play(int channel, string animName, WrapMode wrapMode = WrapMode.Default, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, float weight = 1.0f, bool queued = false)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				//If queued then store the animation queued after, wait for it to be -blend time from end then play non queued with blend time.
				if (queued)
				{
					if (channelGroup != null && IsChannelLayerPlaying(channelGroup._primaryLayer))
					{
						channelGroup._queuedAnimation = animName;
						channelGroup._queuedAnimationWeight = weight;
						channelGroup._queuedAnimationBlendTime = blendTime;
						channelGroup._queuedAnimationEase = easeType;
						channelGroup._queuedAnimationWrapMode = wrapMode;
						return;
					}
				}

				//If no group exists, add new one and return first track index
				if (channelGroup == null)
				{
					channelGroup = new ChannelGroup(channel);
					_channels.Add(channelGroup);
				}
				//Otherwise check an animation is currently playing on this group
				else
				{
					//If not blending stop all animation in this channel
					if (blendTime <= 0.0f)
					{
						StopChannel(channelGroup);
					}

					//Clear queue
					channelGroup._queuedAnimation = null;

					//If this group is playing an animation, move it to a background track
					if (channelGroup._state != ChannelGroup.State.Stopped)
					{
						MovePrimaryAnimationToBackgroundTrack(channelGroup);
					}
				}

				//Start animation on primary track
				StopChannelLayer(channelGroup._primaryLayer);
				channelGroup._primaryLayer._animation = StartAnimationInLayer(channelGroup._primaryLayer._layer, animName, wrapMode);

				//if blending start with weight of zero
				if (blendTime > 0.0f)
				{
					channelGroup._state = ChannelGroup.State.BlendingIn;
					channelGroup._lerpT = 0.0f;
					channelGroup._targetWeight = weight;
					channelGroup._lerpSpeed = 1.0f / blendTime;
					channelGroup._lerpEase = easeType;

					if (channelGroup._primaryLayer._animation != null)
						channelGroup._primaryLayer._animation.weight = 0.0f;
				}
				else
				{
					channelGroup._state = ChannelGroup.State.Playing;

					if (channelGroup._primaryLayer._animation != null)
						channelGroup._primaryLayer._animation.weight = weight;
				}
			}

			public void Stop(int channel, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (blendTime > 0.0f)
					{
						channelGroup._state = ChannelGroup.State.Stopping;
						channelGroup._lerpSpeed = 1.0f / blendTime;
						channelGroup._lerpT = 0.0f;
						channelGroup._lerpEase = easeType;

						if (IsChannelLayerPlaying(channelGroup._primaryLayer))
							channelGroup._primaryLayer._origWeight = channelGroup._primaryLayer._animation.weight;

						for (int i = 0; i < channelGroup._backgroundLayers.Length; i++)
						{
							if (IsChannelLayerPlaying(channelGroup._backgroundLayers[i]))
								channelGroup._backgroundLayers[i]._origWeight = channelGroup._backgroundLayers[i]._animation.weight;
						}
					}
					else
					{
						StopChannel(channelGroup);
					}
				}
			}

			public void StopAll()
			{
				foreach (ChannelGroup channelGroup in _channels)
				{
					StopChannel(channelGroup);
				}
			}

			public void SetAnimationTime(int channel, string animName, float time)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						channelGroup._primaryLayer._animation.time = time;
					}
				}
			}

			public void SetAnimationNormalizedTime(int channel, string animName, float time)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						channelGroup._primaryLayer._animation.normalizedTime = time;
					}
				}
			}

			public void SetAnimationSpeed(int channel, string animName, float speed)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						channelGroup._primaryLayer._animation.speed = speed;
					}
				}
			}

			public void SetAnimationNormalizedSpeed(int channel, string animName, float speed)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						channelGroup._primaryLayer._animation.normalizedSpeed = speed;
					}
				}
			}

			public void SetAnimationWeight(int channel, string animName, float weight)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						channelGroup._primaryLayer._animation.weight = weight;
					}
				}
			}

			public bool IsPlaying(int channel, string animName)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					return IsChannelLayerPlaying(channelGroup._primaryLayer, animName);
				}

				return false;
			}

			public bool DoesAnimationExist(string animName)
			{
				return AnimationComponent[animName] != null;
			}

			public float GetAnimationLength(string animName)
			{
				return AnimationComponent[animName].length;
			}

			public float GetAnimationTime(int channel, string animName)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						return channelGroup._primaryLayer._animation.time;
					}
				}

				return 0f;
			}

			public float GetAnimationNormalizedTime(int channel, string animName)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						return channelGroup._primaryLayer._animation.normalizedTime;
					}
				}

				return 0f;
			}

			public float GetAnimationSpeed(int channel, string animName)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						return channelGroup._primaryLayer._animation.speed;
					}
				}

				return 1f;
			}

			public float GetAnimationNormalizedSpeed(int channel, string animName)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						return channelGroup._primaryLayer._animation.normalizedSpeed;
					}
				}

				return 0f;
			}

			public float GetAnimationWeight(int channel, string animName)
			{
				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup != null)
				{
					if (IsChannelLayerPlaying(channelGroup._primaryLayer, animName))
					{
						return channelGroup._primaryLayer._animation.weight;
					}
				}

				return 0f;
			}

#if UNITY_EDITOR
			public string[] GetAnimationNames()
			{
				string[] animationNames = new string[AnimationComponent.GetClipCount()];
				int index = 0;
				foreach (AnimationState animationState in AnimationComponent)
				{
					animationNames[index++] = animationState.clip.name;
				}

				return animationNames;
			}
#endif
			#endregion

			#region Public Interface
			public void SetChannelData(int channel, AnimationParams primaryAnim, params AnimationParams[] backgroundAnims)
			{
				UnityEngine.Debug.Log(" a:" + primaryAnim._animName + " w:" + primaryAnim._weight + " t:" + primaryAnim._time);

				ChannelGroup channelGroup = GetChannelGroup(channel);

				if (channelGroup == null)
				{
					channelGroup = new ChannelGroup(channel);
					_channels.Add(channelGroup);
				}

				if (string.IsNullOrEmpty(primaryAnim._animName))
				{
					StopChannel(channelGroup);
					return;
				}				
				else if (!IsChannelLayerPlaying(channelGroup._primaryLayer, primaryAnim._animName))
				{
					StopChannelLayer(channelGroup._primaryLayer);
					channelGroup._primaryLayer._animation = StartAnimationInLayer(channelGroup._primaryLayer._layer, primaryAnim._animName, WrapMode.Default);
				}

				channelGroup._state = ChannelGroup.State.Playing;
				channelGroup._primaryLayer._animation.time = primaryAnim._time;
				channelGroup._primaryLayer._animation.speed = primaryAnim._speed;
				channelGroup._primaryLayer._animation.weight = primaryAnim._weight;

				for (int i=0; i<kNumberOfBackgroundLayers; i++)
				{
					if (i < backgroundAnims.Length)
					{
						if (!IsChannelLayerPlaying(channelGroup._backgroundLayers[i], backgroundAnims[i]._animName))
						{
							StopChannelLayer(channelGroup._backgroundLayers[i]);
							channelGroup._backgroundLayers[i]._animation = StartAnimationInLayer(channelGroup._backgroundLayers[i]._layer, backgroundAnims[i]._animName, WrapMode.Default);
						}

						channelGroup._backgroundLayers[i]._animation.time = backgroundAnims[i]._time;
						channelGroup._backgroundLayers[i]._animation.speed = backgroundAnims[i]._speed;
						channelGroup._backgroundLayers[i]._animation.weight = backgroundAnims[i]._weight;
					}
					else
					{
						StopChannelLayer(channelGroup._backgroundLayers[i]);	
					}
				}
			}
			
			public AnimationClip GetClip(string animName)
			{
				return AnimationComponent.GetClip(animName);
			}
			#endregion

			#region Private functions
			private void UpdateChannelBlends(ChannelGroup channelGroup)
			{
				//Check primary animation is still valid, if not set channel to stopped
				if (channelGroup._state != ChannelGroup.State.Stopped && !IsChannelLayerPlaying(channelGroup._primaryLayer))
				{
					StopChannel(channelGroup);
				}

				switch (channelGroup._state)
				{
					//If fading in primary track...
					case ChannelGroup.State.BlendingIn:
						{
							channelGroup._lerpT += channelGroup._lerpSpeed * Time.deltaTime;

							if (channelGroup._lerpT >= 1.0f)
							{
								channelGroup._primaryLayer._animation.weight = channelGroup._targetWeight;

								for (int i = 0; i < channelGroup._backgroundLayers.Length; i++)
								{
									StopChannelLayer(channelGroup._backgroundLayers[i]);
								}

								channelGroup._state = ChannelGroup.State.Playing;
							}
							else
							{
								channelGroup._primaryLayer._animation.weight = MathUtils.Interpolate(channelGroup._lerpEase, 0f, channelGroup._targetWeight, channelGroup._lerpT);
							}
						}
						break;


					case ChannelGroup.State.Stopping:
						{
							channelGroup._lerpT += channelGroup._lerpSpeed * Time.deltaTime;

							if (channelGroup._lerpT >= 1.0f)
							{
								StopChannel(channelGroup);
								channelGroup._state = ChannelGroup.State.Stopped;
							}
							else
							{
								channelGroup._primaryLayer._animation.weight = MathUtils.Interpolate(channelGroup._lerpEase, channelGroup._primaryLayer._origWeight, 0f, channelGroup._lerpT);

								for (int i = 0; i < channelGroup._backgroundLayers.Length; i++)
								{
									if (IsChannelLayerPlaying(channelGroup._backgroundLayers[i]))
										channelGroup._backgroundLayers[i]._animation.weight = MathUtils.Interpolate(channelGroup._lerpEase, channelGroup._backgroundLayers[i]._origWeight, 0f, channelGroup._lerpT);
								}
							}
						}
						break;
				}

				//If have a queued animation,
				if (!string.IsNullOrEmpty(channelGroup._queuedAnimation))
				{
					bool queuedReady;
					float timeRemaining;

					if (IsChannelLayerPlaying(channelGroup._primaryLayer))
					{
						timeRemaining = channelGroup._primaryLayer._animation.length - channelGroup._primaryLayer._animation.time;
						queuedReady = timeRemaining <= channelGroup._queuedAnimationBlendTime;
					}
					else
					{
						queuedReady = true;
						timeRemaining = 0f;
					}

					if (queuedReady)
					{
						Play(channelGroup._channel, channelGroup._queuedAnimation, channelGroup._queuedAnimationWrapMode, timeRemaining, channelGroup._queuedAnimationEase, channelGroup._queuedAnimationWeight);
					}
				}
			}

			private ChannelGroup GetChannelGroup(int channel)
			{
				foreach (ChannelGroup channelGroup in _channels)
				{
					if (channelGroup._channel == channel)
						return channelGroup;
				}

				return null;
			}

			private void StopChannelLayer(ChannelLayer layer)
			{
				if (layer._animation != null && layer._animation.layer == layer._layer)
				{
					layer._animation.enabled = false;
					layer._animation.weight = 0f;
				}
					
				layer._animation = null;
			}

			private void StopChannel(ChannelGroup channelGroup)
			{
				if (channelGroup != null)
				{
					StopChannelLayer(channelGroup._primaryLayer);

					for (int i = 0; i < channelGroup._backgroundLayers.Length; i++)
					{
						StopChannelLayer(channelGroup._backgroundLayers[i]);
					}
				}
			}

			private AnimationState StartAnimationInLayer(int layer, string animName, WrapMode wrapMode)
			{
				AnimationState animation = AnimationComponent[animName];

				if (animation != null)
				{
					animation.layer = layer;

					if (wrapMode == WrapMode.Default)
					{
						if (AnimationComponent.wrapMode == WrapMode.Default)
							wrapMode = animation.clip.wrapMode;
						else
							wrapMode = AnimationComponent.wrapMode;
					}

					animation.wrapMode = wrapMode;
					animation.time = 0f;
					animation.enabled = true;
				}
				
				return animation;
			}


			private void MovePrimaryAnimationToBackgroundTrack(ChannelGroup channelGroup)
			{
				AnimationState lastLayerAnimation;
				float lastLayerOrigWeight;

				if (IsChannelLayerPlaying(channelGroup._primaryLayer))
				{
					channelGroup._primaryLayer._animation.layer -= 1;
					lastLayerAnimation = channelGroup._primaryLayer._animation;
					lastLayerOrigWeight = channelGroup._primaryLayer._origWeight;
				}
				else
				{
					lastLayerAnimation = null;
					lastLayerOrigWeight = 0f;
				}

				for (int i=0; i < kNumberOfBackgroundLayers ; i++)
				{
					AnimationState animation = null;
					float origWeight = 0f;

					if (IsChannelLayerPlaying(channelGroup._backgroundLayers[i]))
					{
						if (i < kNumberOfBackgroundLayers - 1)
						{
							channelGroup._backgroundLayers[i]._animation.layer -= 1;
							animation = channelGroup._backgroundLayers[i]._animation;
							origWeight = channelGroup._backgroundLayers[i]._origWeight;
						}
						else
						{
							StopChannelLayer(channelGroup._backgroundLayers[i]);
						}
					}

					//Set new animation
					channelGroup._backgroundLayers[i]._animation = lastLayerAnimation;
					if (lastLayerAnimation != null)
						lastLayerAnimation.layer = channelGroup._backgroundLayers[i]._layer;
					channelGroup._backgroundLayers[i]._origWeight = lastLayerOrigWeight;

					lastLayerAnimation = animation;
					lastLayerOrigWeight = origWeight;
				}
			}

			private bool IsChannelLayerPlaying(ChannelLayer channelLayer)
			{
				return channelLayer._animation != null && channelLayer._animation.enabled && channelLayer._animation.layer == channelLayer._layer;
			}

			private bool IsChannelLayerPlaying(ChannelLayer channelLayer, string animName)
			{
				return IsChannelLayerPlaying(channelLayer) && channelLayer._animation.name == animName;
			}
			#endregion
		}
	}

}