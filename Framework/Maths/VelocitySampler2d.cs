using System;
using UnityEngine;

namespace Framework
{
	namespace Maths
	{
		public class VelocitySampler2d
		{
			private struct PositionSample
			{
				public Vector2 _position;
				public float _time;
				public Vector2 _velocity;
			}

			private PositionSample[] _positionSamples;
			private int _currentSampleIndex;
			private int _numSamples;

			#region Public Interface
			public VelocitySampler2d(int numSamples)
			{
				_positionSamples = new PositionSample[numSamples];
				Clear();
			}

			public void AddPosition(Vector2 position)
			{
				_numSamples = Math.Min(_numSamples + 1, _positionSamples.Length);

				float currentTime = GetTime();

				int prevIndex = _currentSampleIndex;
				_currentSampleIndex = (_currentSampleIndex + 1) % _positionSamples.Length;

				_positionSamples[_currentSampleIndex]._position = position;
				_positionSamples[_currentSampleIndex]._time = currentTime;

				//If have at least one previous sample work out velocity
				if (_numSamples > 1)
				{
					//Find 
					float timeDelta = currentTime - _positionSamples[prevIndex]._time;

					if (timeDelta > 0f)
						_positionSamples[_currentSampleIndex]._velocity = (position - _positionSamples[prevIndex]._position) / timeDelta;
					else
						_positionSamples[_currentSampleIndex]._velocity = Vector2.zero;

					//If this is the second sample set first sample velocity to match this (will otherwise be zero)
					if (_numSamples == 2)
					{
						_positionSamples[prevIndex]._velocity = _positionSamples[_currentSampleIndex]._velocity;
					}
				}
			}

			public void Clear()
			{
				_numSamples = 0;
				_currentSampleIndex = 0;
			}

			public Vector2 GetAvgVelocity(float timePeriod)
			{
				Vector2 avgVelocity = Vector2.zero;

				//Need more than one position to work out velocity
				if (_numSamples > 1)
				{
					float avgVelocitySamplePeriod = 0f;
					float prevSampleTime = GetTime();

					int sample = _currentSampleIndex;
					int count = 0;

					while (count < _numSamples)
					{
						float sampleTimeDelta = prevSampleTime - _positionSamples[sample]._time;

						//not got a sample yet
						if (avgVelocitySamplePeriod <= 0f)
						{
							avgVelocity = _positionSamples[sample]._velocity;
							avgVelocitySamplePeriod = sampleTimeDelta;
						}
						else
						{
							float newSamplePeriod = avgVelocitySamplePeriod + sampleTimeDelta;
							avgVelocity = (avgVelocity * avgVelocitySamplePeriod) / newSamplePeriod + (_positionSamples[sample]._velocity * sampleTimeDelta) / newSamplePeriod;
							avgVelocitySamplePeriod = newSamplePeriod;
						}

						//If sample period is now long enough break
						if (avgVelocitySamplePeriod >= timePeriod)
						{
							break;
						}

						//Move to previous sample
						prevSampleTime = _positionSamples[sample]._time;
						sample = GetPrevIndex(sample);
						count++;
					}
				}

				return avgVelocity;
			}
			#endregion

			private float GetTime()
			{
				return Time.time;
			}

			private int GetPrevIndex(int index)
			{
				index--;

				if (index < 0)
				{
					index = _positionSamples.Length + index;
				}

				return index;
			}
		}
	}
}
