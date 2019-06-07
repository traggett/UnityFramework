using UnityEngine;

namespace Framework
{
	using Utils;

	namespace MeshInstancing
	{
		namespace GPUAnimations
		{
			[RequireComponent(typeof(Animation))]
			public class GPUAnimation : GPUAnimatorBase
			{
				#region Public Data
				public WrapMode _wrapMode;
				public string _clip;
				//public AnimationState this[string name] { get; }
				#endregion

				#region Private Data
				private SkinnedMeshRenderer _skinnedMeshRenderer;
				private int _currentPlayerIndex;
				private GPUAnimationPlayer[] _clipPlayers;
				private AnimationState[] _animationStates;
				private float _currentAnimationWeight;
				#endregion

				#region MonoBehaviour
				private void Awake()
				{
					_skinnedMeshRenderer = GameObjectUtils.GetComponent<SkinnedMeshRenderer>(this.gameObject, true);
					_clipPlayers = new GPUAnimationPlayer[2];
					_animationStates = new AnimationState[2];
					_currentPlayerIndex = 0;
					_currentAnimationWeight = 1.0f;

					_onInitialise += Initialise;

					UpdateCachedTransform();
				}

				private void Update()
				{
					UpdateAnimations();
				}
				#endregion

				#region Public Interface
				public bool Play(PlayMode mode = PlayMode.StopSameLayer)
				{
					return Play(_clip, mode);
				}

				public bool Play(string animName, PlayMode mode = PlayMode.StopSameLayer)
				{
					if (GetAnimation(animName, out GPUAnimations.Animation animation))
					{
						Play(animation, _currentPlayerIndex);
						Stop(1 - _currentPlayerIndex);
						_currentAnimationWeight = 1.0f;
						return true;
					}
					else
					{
						Stop(_currentPlayerIndex);
						Stop(1 - _currentPlayerIndex);
						_currentAnimationWeight = 1.0f;
						return false;
					}
				}

				public AnimationState PlayQueued(string animName, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					return new AnimationState();
				}

				public AnimationState CrossFade(string animName, float fadeLength = 0.3f, PlayMode mode = PlayMode.StopSameLayer)
				{
					return new AnimationState();
				}

				public AnimationState CrossFadeQueued(string animName, float fadeLength = 0.3f, QueueMode queue = QueueMode.CompleteOthers, PlayMode mode = PlayMode.StopSameLayer)
				{
					return new AnimationState();
				}

				public void Blend(string animName, float targetWeight = 1.0f, float fadeLength = 0.3f)
				{

				}

				public bool IsPlaying(string animName)
				{
					return true;
				}
				#endregion

				#region GPUAnimatorBase
				public override float GetCurrentAnimationFrame()
				{
					return _clipPlayers[_currentPlayerIndex].GetCurrentTexureFrame();
				}

				public override float GetCurrentAnimationWeight()
				{
					return _currentAnimationWeight;
				}

				public override float GetPreviousAnimationFrame()
				{
					return _clipPlayers[1 - _currentPlayerIndex].GetCurrentTexureFrame();
				}

				public override Bounds GetBounds()
				{
					if (_skinnedMeshRenderer != null)
						return _skinnedMeshRenderer.bounds;

					return new Bounds();
				}
				#endregion

				#region Private Functions
				private void Initialise()
				{
					_clipPlayers[0].Stop();
					_clipPlayers[1].Stop();
					_currentPlayerIndex = 0;
					_currentAnimationWeight = 1.0f;
				}
				
				private void UpdateAnimations()
				{
					_clipPlayers[0].Update(Time.deltaTime);
					_clipPlayers[1].Update(Time.deltaTime);

					//Update queueing / blending
				}

				private bool GetAnimation(string animationName, out GPUAnimations.Animation animation)
				{
					GPUAnimations animations = _renderer._animationTexture.GetAnimations();

					for (int i=0; i< animations._animations.Length; i++)
					{
						if (animations._animations[i]._name == animationName)
						{
							animation = animations._animations[i];
							return true;
						}
					}

					animation = GPUAnimations.Animation.kInvalid;
					return false;
				}

				private void Play(GPUAnimations.Animation animation, int playerIndex)
				{
					_clipPlayers[playerIndex].Play(this.gameObject, animation);

					
					_animationStates[playerIndex].enabled = false;
					_animationStates[playerIndex].weight = 1.0f;
					_animationStates[playerIndex].wrapMode = animation._wrapMode;			
					_animationStates[playerIndex].time = 0.0f;
					_animationStates[playerIndex].normalizedTime = 0.0f;
					_animationStates[playerIndex].speed = 1.0f;
					_animationStates[playerIndex].normalizedSpeed = 1.0f;
					_animationStates[playerIndex].layer = 0;
					_animationStates[playerIndex].name = animation._name;
					_animationStates[playerIndex].blendMode = AnimationBlendMode.Blend;
				}

				private void Stop(int playerIndex)
				{
					_clipPlayers[playerIndex].Stop();
				}
				#endregion
			}
		}
    }
}


//Could make states for all animations
//Then update current two animations based on these?
//Yeah ok
//So play / blend etc updates all states

//Then logic only grabs top two