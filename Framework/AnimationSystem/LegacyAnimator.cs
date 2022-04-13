using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	using Maths;

	namespace AnimationSystem
	{
		[RequireComponent(typeof(Animation))]
		public class LegacyAnimator : MonoBehaviour
		{
			#region Public Data
			public struct AnimationParams
			{
				public AnimationClip _animation;
				public float _time;
				public float _speed;
				public float _weight;
			}

			//The number of animation that can be blending at any one time per each layer
			public static readonly int MaxBackgroundLayers = 2;
			#endregion

			#region Private Data
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
			private struct LayerChannel
			{
				//Layer in animation component
				public readonly int _layer;
				//The current animation
				public AnimationState _animation;
				//Used for blending when stopping
				public float _origWeight;		
				
				public LayerChannel(int layer)
				{
					_layer = layer;
					_animation = null;
					_origWeight = 1f;
				}
			}

			private class LayerGroup
			{
				public enum State
				{
					Stopped,
					Playing,
					BlendingIn,
					Stopping,
				}
				public State _state = State.Stopped;

				public readonly int _layer;
				public LayerChannel _primaryLayer;
				public LayerChannel[] _backgroundLayers;

				//Primary track blending data
				public float _lerpT;
				public float _lerpSpeed;
				public InterpolationType _lerpEase;
				public float _targetWeight;
				public PlayMode _playMode;

				//Queued animation data
				public string _queuedAnimation;
				public float _queuedAnimationWeight;
				public float _queuedAnimationBlendTime;
				public WrapMode _queuedAnimationWrapMode;
				public InterpolationType _queuedAnimationEase;
				public PlayMode _queuedPlayMode;

				public LayerGroup(int layer)
				{
					_layer = layer;
					_primaryLayer = new LayerChannel((layer * (MaxBackgroundLayers + 1)) + MaxBackgroundLayers);
					_backgroundLayers = new LayerChannel[MaxBackgroundLayers];
					for (int i = 0; i<MaxBackgroundLayers; i++)
					{
						_backgroundLayers[i] = new LayerChannel(_primaryLayer._layer - i - 1);
					}
				}
			}
			private readonly List<LayerGroup> _layers = new List<LayerGroup>();
			#endregion

			#region Unity Messages
			void Update()
			{
				foreach (LayerGroup layerGroup in _layers)
				{
					UpdatelayerBlends(layerGroup);
				}
			}
			#endregion

			#region Public Interface
			public void Play(string animName, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, PlayMode playMode = PlayMode.StopSameLayer)
			{
				Play(0, animName, blendTime, easeType, playMode);
			}

			public void Play(int layer, string animName, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, PlayMode playMode = PlayMode.StopSameLayer, WrapMode wrapMode = WrapMode.Default, float weight = 1.0f, bool queued = false)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				//If queued then store the animation queued after, wait for it to be -blend time from end then play non queued with blend time.
				if (queued)
				{
					if (layerGroup != null && IsLayerChannelPlaying(layerGroup._primaryLayer))
					{
						layerGroup._queuedAnimation = animName;
						layerGroup._queuedAnimationWeight = weight;
						layerGroup._queuedAnimationBlendTime = blendTime;
						layerGroup._queuedAnimationEase = easeType;
						layerGroup._queuedAnimationWrapMode = wrapMode;
						layerGroup._queuedPlayMode = playMode;
						return;
					}
				}

				//If no group exists, add new one and return first track index
				if (layerGroup == null)
				{
					layerGroup = new LayerGroup(layer);
					_layers.Add(layerGroup);
				}
				//Otherwise check an animation is currently playing on this group
				else
				{
					//If not blending stop all animation in this layer
					if (blendTime <= 0.0f)
					{
						StopLayer(layerGroup);
					}

					//Clear queue
					layerGroup._queuedAnimation = null;

					//If this group is playing an animation, move it to a background track
					if (layerGroup._state != LayerGroup.State.Stopped)
					{
						MovePrimaryAnimationToBackgroundTrack(layerGroup);
					}
				}

				//Start animation on primary track
				StopLayerChannel(layerGroup._primaryLayer);
				layerGroup._primaryLayer._animation = StartAnimationInLayer(layerGroup._primaryLayer._layer, animName, wrapMode);

				if (layerGroup._primaryLayer._animation != null)
				{
					//if blending start with weight of zero
					if (blendTime > 0.0f)
					{
						layerGroup._state = LayerGroup.State.BlendingIn;
						layerGroup._lerpT = 0.0f;
						layerGroup._targetWeight = weight;
						layerGroup._lerpSpeed = 1.0f / blendTime;
						layerGroup._lerpEase = easeType;
						layerGroup._playMode = playMode;

						if (layerGroup._primaryLayer._animation != null)
							layerGroup._primaryLayer._animation.weight = 0.0f;
					}
					else
					{
						layerGroup._state = LayerGroup.State.Playing;

						if (layerGroup._primaryLayer._animation != null)
							layerGroup._primaryLayer._animation.weight = weight;


						//If mode is stop all, stop all other layers
						if (playMode == PlayMode.StopAll)
						{
							foreach (LayerGroup group in _layers)
							{
								if (group != layerGroup)
									StopLayer(group);
							}
						}
					}
				}
			}

			public void PlayQueued(string animName, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, PlayMode playMode = PlayMode.StopSameLayer)
			{
				PlayQueued(0, animName, blendTime, easeType, playMode);
			}

			public void PlayQueued(int layer, string animName, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, PlayMode playMode = PlayMode.StopSameLayer)
			{
				Play(layer, animName, blendTime, easeType, playMode, queued: true);
			}

			public void Stop(float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, PlayMode playMode = PlayMode.StopSameLayer)
			{
				Stop(0, blendTime, easeType, playMode);
			}

			public void Stop(int layer, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, PlayMode playMode = PlayMode.StopSameLayer)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (blendTime > 0.0f)
					{
						layerGroup._state = LayerGroup.State.Stopping;
						layerGroup._lerpSpeed = 1.0f / blendTime;
						layerGroup._lerpT = 0.0f;
						layerGroup._lerpEase = easeType;
						layerGroup._playMode = playMode;

						if (IsLayerChannelPlaying(layerGroup._primaryLayer))
							layerGroup._primaryLayer._origWeight = layerGroup._primaryLayer._animation.weight;

						for (int i = 0; i < layerGroup._backgroundLayers.Length; i++)
						{
							if (IsLayerChannelPlaying(layerGroup._backgroundLayers[i]))
								layerGroup._backgroundLayers[i]._origWeight = layerGroup._backgroundLayers[i]._animation.weight;
						}
					}
					else
					{
						StopLayer(layerGroup);
					}
				}
			}

			public void StopAll()
			{
				foreach (LayerGroup layerGroup in _layers)
				{
					StopLayer(layerGroup);
				}
			}

			public void SetAnimationTime(string animName, float time, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						layerGroup._primaryLayer._animation.time = time;
					}
				}
			}

			public void SetAnimationNormalizedTime(string animName, float time, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						layerGroup._primaryLayer._animation.normalizedTime = time;
					}
				}
			}

			public void SetAnimationSpeed(string animName, float speed, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						layerGroup._primaryLayer._animation.speed = speed;
					}
				}
			}

			public void SetAnimationNormalizedSpeed(string animName, float speed, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						layerGroup._primaryLayer._animation.normalizedSpeed = speed;
					}
				}
			}

			public void SetAnimationWeight(string animName, float weight, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						layerGroup._primaryLayer._animation.weight = weight;
					}
				}
			}

			public bool IsPlaying(string animName, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					return IsLayerChannelPlaying(layerGroup._primaryLayer, animName);
				}

				return false;
			}

			public bool DoesAnimationExist(string animName, int layer = 0)
			{
				return AnimationComponent[animName] != null;
			}

			public float GetAnimationLength(string animName, int layer = 0)
			{
				return AnimationComponent[animName].length;
			}

			public float GetAnimationTime(string animName, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						return layerGroup._primaryLayer._animation.time;
					}
				}

				return 0f;
			}

			public float GetAnimationNormalizedTime(string animName, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						return layerGroup._primaryLayer._animation.normalizedTime;
					}
				}

				return 0f;
			}

			public float GetAnimationSpeed(string animName, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						return layerGroup._primaryLayer._animation.speed;
					}
				}

				return 1f;
			}

			public float GetAnimationNormalizedSpeed(string animName, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						return layerGroup._primaryLayer._animation.normalizedSpeed;
					}
				}

				return 0f;
			}

			public float GetAnimationWeight(string animName, int layer = 0)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup != null)
				{
					if (IsLayerChannelPlaying(layerGroup._primaryLayer, animName))
					{
						return layerGroup._primaryLayer._animation.weight;
					}
				}

				return 0f;
			}

			public void Sample()
			{
				AnimationComponent.Sample();
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
			public void SetLayerData(int layer, AnimationParams primaryAnim, params AnimationParams[] backgroundAnims)
			{
				LayerGroup layerGroup = GetLayerGroup(layer);

				if (layerGroup == null)
				{
					layerGroup = new LayerGroup(layer);
					_layers.Add(layerGroup);
				}

				if (primaryAnim._animation == null)
				{
					StopLayer(layerGroup);
					return;
				}

				if (AddClipIfDoesntExist(primaryAnim._animation) || !IsLayerChannelPlaying(layerGroup._primaryLayer, primaryAnim._animation.name))
				{
					StopLayerChannel(layerGroup._primaryLayer);
					layerGroup._primaryLayer._animation = StartAnimationInLayer(layerGroup._primaryLayer._layer, primaryAnim._animation.name, WrapMode.Default);
				}

				layerGroup._state = LayerGroup.State.Playing;
				layerGroup._primaryLayer._animation.time = primaryAnim._time;
				layerGroup._primaryLayer._animation.speed = primaryAnim._speed;
				layerGroup._primaryLayer._animation.weight = primaryAnim._weight;

				for (int i=0; i<MaxBackgroundLayers; i++)
				{
					if (i < backgroundAnims.Length)
					{
						if (AddClipIfDoesntExist(backgroundAnims[i]._animation) || !IsLayerChannelPlaying(layerGroup._backgroundLayers[i], backgroundAnims[i]._animation.name))
						{
							StopLayerChannel(layerGroup._backgroundLayers[i]);
							layerGroup._backgroundLayers[i]._animation = StartAnimationInLayer(layerGroup._backgroundLayers[i]._layer, backgroundAnims[i]._animation.name, WrapMode.Default);
						}

						layerGroup._backgroundLayers[i]._animation.time = backgroundAnims[i]._time;
						layerGroup._backgroundLayers[i]._animation.speed = backgroundAnims[i]._speed;
						layerGroup._backgroundLayers[i]._animation.weight = backgroundAnims[i]._weight;
					}
					else
					{
						StopLayerChannel(layerGroup._backgroundLayers[i]);	
					}
				}
			}
			#endregion

			#region Private functions
			private void UpdatelayerBlends(LayerGroup layerGroup)
			{
				//Check primary animation is still valid, if not set layer to stopped
				if (layerGroup._state != LayerGroup.State.Stopped && !IsLayerChannelPlaying(layerGroup._primaryLayer))
				{
					StopLayer(layerGroup);
				}

				switch (layerGroup._state)
				{
					//If fading in primary track...
					case LayerGroup.State.BlendingIn:
						{
							layerGroup._lerpT += layerGroup._lerpSpeed * Time.deltaTime;

							if (layerGroup._lerpT >= 1.0f)
							{
								layerGroup._primaryLayer._animation.weight = layerGroup._targetWeight;

								for (int i = 0; i < layerGroup._backgroundLayers.Length; i++)
								{
									StopLayerChannel(layerGroup._backgroundLayers[i]);
								}

								layerGroup._state = LayerGroup.State.Playing;

								//If mode is stop all, stop all other layers
								if (layerGroup._playMode == PlayMode.StopAll)
								{
									foreach (LayerGroup group in _layers)
									{
										if (group != layerGroup)
											StopLayer(group);
									}
								}
							}
							else
							{
								layerGroup._primaryLayer._animation.weight = MathUtils.Interpolate(layerGroup._lerpEase, 0f, layerGroup._targetWeight, layerGroup._lerpT);
							}
						}
						break;


					case LayerGroup.State.Stopping:
						{
							layerGroup._lerpT += layerGroup._lerpSpeed * Time.deltaTime;

							if (layerGroup._lerpT >= 1.0f)
							{
								layerGroup._state = LayerGroup.State.Stopped;

								if (layerGroup._playMode == PlayMode.StopAll)
								{
									StopAll();
								}
								else
								{
									StopLayer(layerGroup);
								}
							}
							else
							{
								layerGroup._primaryLayer._animation.weight = MathUtils.Interpolate(layerGroup._lerpEase, layerGroup._primaryLayer._origWeight, 0f, layerGroup._lerpT);

								for (int i = 0; i < layerGroup._backgroundLayers.Length; i++)
								{
									if (IsLayerChannelPlaying(layerGroup._backgroundLayers[i]))
										layerGroup._backgroundLayers[i]._animation.weight = MathUtils.Interpolate(layerGroup._lerpEase, layerGroup._backgroundLayers[i]._origWeight, 0f, layerGroup._lerpT);
								}
							}
						}
						break;
				}

				//If have a queued animation,
				if (!string.IsNullOrEmpty(layerGroup._queuedAnimation))
				{
					bool queuedReady;
					float timeRemaining;

					if (IsLayerChannelPlaying(layerGroup._primaryLayer))
					{
						timeRemaining = layerGroup._primaryLayer._animation.length - layerGroup._primaryLayer._animation.time;
						queuedReady = timeRemaining <= layerGroup._queuedAnimationBlendTime;
					}
					else
					{
						queuedReady = true;
						timeRemaining = 0f;
					}

					if (queuedReady)
					{
						Play(layerGroup._layer, layerGroup._queuedAnimation, timeRemaining, layerGroup._queuedAnimationEase, 
								layerGroup._queuedPlayMode, layerGroup._queuedAnimationWrapMode, layerGroup._queuedAnimationWeight);
					}
				}
			}

			private LayerGroup GetLayerGroup(int layer)
			{
				foreach (LayerGroup layerGroup in _layers)
				{
					if (layerGroup._layer == layer)
						return layerGroup;
				}

				return null;
			}

			private void StopLayerChannel(LayerChannel layer)
			{
				if (layer._animation != null && layer._animation.layer == layer._layer)
				{
					layer._animation.enabled = false;
					layer._animation.weight = 0f;
				}
					
				layer._animation = null;
			}

			private void StopLayer(LayerGroup layerGroup)
			{
				if (layerGroup != null)
				{
					StopLayerChannel(layerGroup._primaryLayer);

					for (int i = 0; i < layerGroup._backgroundLayers.Length; i++)
					{
						StopLayerChannel(layerGroup._backgroundLayers[i]);
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


			private void MovePrimaryAnimationToBackgroundTrack(LayerGroup layerGroup)
			{
				AnimationState lastLayerAnimation;
				float lastLayerOrigWeight;

				if (IsLayerChannelPlaying(layerGroup._primaryLayer))
				{
					layerGroup._primaryLayer._animation.layer -= 1;
					lastLayerAnimation = layerGroup._primaryLayer._animation;
					lastLayerOrigWeight = layerGroup._primaryLayer._origWeight;
				}
				else
				{
					lastLayerAnimation = null;
					lastLayerOrigWeight = 0f;
				}

				for (int i=0; i < MaxBackgroundLayers ; i++)
				{
					AnimationState animation = null;
					float origWeight = 0f;

					if (IsLayerChannelPlaying(layerGroup._backgroundLayers[i]))
					{
						if (i < MaxBackgroundLayers - 1)
						{
							layerGroup._backgroundLayers[i]._animation.layer -= 1;
							animation = layerGroup._backgroundLayers[i]._animation;
							origWeight = layerGroup._backgroundLayers[i]._origWeight;
						}
						else
						{
							StopLayerChannel(layerGroup._backgroundLayers[i]);
						}
					}

					//Set new animation
					layerGroup._backgroundLayers[i]._animation = lastLayerAnimation;
					if (lastLayerAnimation != null)
						lastLayerAnimation.layer = layerGroup._backgroundLayers[i]._layer;
					layerGroup._backgroundLayers[i]._origWeight = lastLayerOrigWeight;

					lastLayerAnimation = animation;
					lastLayerOrigWeight = origWeight;
				}
			}

			private bool IsLayerChannelPlaying(LayerChannel layerLayer)
			{
				return layerLayer._animation != null && layerLayer._animation.enabled && layerLayer._animation.layer == layerLayer._layer;
			}

			private bool IsLayerChannelPlaying(LayerChannel layerLayer, string animName)
			{
				return IsLayerChannelPlaying(layerLayer) && layerLayer._animation.name == animName;
			}

			private bool AddClipIfDoesntExist(AnimationClip clip)
			{
				if (AnimationComponent.GetClip(clip.name) == null)
				{
					AnimationComponent.AddClip(clip, clip.name);
					return true;
				}

				return false;
			}
			#endregion
		}
	}

}