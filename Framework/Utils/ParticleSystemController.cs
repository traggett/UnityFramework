using UnityEngine;

namespace Framework
{
	[RequireComponent(typeof(ParticleSystem))]
	public class ParticleSystemController : MonoBehaviour
	{
		public bool _ignoreTimeScale;

		private enum eState
		{
			Stopped,
			Playing,
			Stopping
		}

		private ParticleSystem _particleSystem;
		private eState _state = eState.Stopped;
		private ParticleSystem.Particle[] _particles;

		void Start()
		{
			_particleSystem = GetComponent<ParticleSystem>();
			_particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];

			if (_particleSystem.main.playOnAwake)
			{
				Play();
			}
		}

		public bool IsPlaying()
		{
			return _state == eState.Playing;
		}

		public void Play()
		{
			_state = eState.Playing;

			ParticleSystem.EmissionModule emissionModule = _particleSystem.emission;
			emissionModule.enabled = true;
			_particleSystem.Play();
			_particleSystem.Pause();
		}

		public void Stop(float fadeTime=0.0f)
		{
			if (_state == eState.Playing)
			{
				if (fadeTime > 0.0f)
				{
					ParticleSystem.EmissionModule emissionModule = _particleSystem.emission;
					emissionModule.enabled = false;

					int numParticlesAlive = _particleSystem.GetParticles(_particles);

					for (int i = 0; i < numParticlesAlive; i++)
					{
						_particles[i].remainingLifetime = fadeTime;
					}
					_particleSystem.SetParticles(_particles, numParticlesAlive);

					_state = eState.Stopping;
				}
				else
				{
					_state = eState.Stopped;
					_particleSystem.Clear(true);
					_particleSystem.Stop();
				}
			}		
		}

		void Update()
		{
			switch (_state)
			{
				case eState.Playing:
					{
						_particleSystem.Simulate(_ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime, true, false);
					}
					break;
				case eState.Stopping:
					{
						_particleSystem.Simulate(_ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime, true, false);

						int numParticlesAlive = _particleSystem.GetParticles(_particles);

						if (numParticlesAlive == 0)
						{
							_particleSystem.Stop();
							_state = eState.Stopped;
						}
					}
					break;
				case eState.Stopped:
					{
					}
					break;
			}
		}
	}
}
