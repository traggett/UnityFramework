using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		public abstract class AnimatorParamTrackMixer<T> : PlayableBehaviour, ITrackMixer, IParentBindableTrackMixer 
		{
			private TrackAsset _trackAsset;
			private PlayableDirector _director;

			protected Animator _trackBinding;
			protected int _parameterHash;

			private bool _parentBinding;
			private bool _firstFrame = true;

			private object _defaultValue;

			protected abstract object GetValue();
			protected abstract void SetValue(object value);
			protected abstract object ApplyValue(object value, float inputWeight, Playable playable);

			public override void ProcessFrame(Playable playable, FrameData info, object playerData)
			{
				if (!_parentBinding)
					_trackBinding = playerData as Animator;

				if (_trackBinding == null)
					return;

				if (_firstFrame)
				{
					AnimatorParamTrack track = (AnimatorParamTrack)_trackAsset;

					_parameterHash = Animator.StringToHash(track._parameterId);
					_defaultValue = GetValue();
					_firstFrame = false;
				}

				object value = _defaultValue;

				int numInputs = playable.GetInputCount();

				for (int i = 0; i < numInputs; i++)
				{
					float inputWeight = playable.GetInputWeight(i);

					if (inputWeight > 0.0f)
					{
						value = ApplyValue(value, inputWeight, playable.GetInput(i));
					}
				}

				SetValue(value);
			}

			public override void OnPlayableDestroy(Playable playable)
			{
				_firstFrame = true;

				if (_trackBinding == null)
					return;

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

			#region IParentBindable
			public void SetBinding(object playerData)
			{
				if (playerData is GameObject)
				{
					_trackBinding = ((GameObject)playerData).GetComponent<Animator>();
					_parentBinding = true;
				}
				else if (playerData is Transform)
				{
					_trackBinding = ((Transform)playerData).gameObject.GetComponent<Animator>();
					_parentBinding = true;
				}
				else
				{
					_parentBinding = false;
				}
			}
			#endregion
		}
	}
}
