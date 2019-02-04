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

				AnimatorParamTrack track = (AnimatorParamTrack)_trackAsset;

				if (_firstFrame)
				{
					_parameterHash = GetHash(track._parameterId);
					_defaultValue = GetValue();
					_firstFrame = false;
				}

#if UNITY_EDITOR
				//Always update hash in editor
				if (Application.isEditor)
					_parameterHash = GetHash(track._parameterId);
#endif

				if (_parameterHash == -1)
					return;

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
					_trackBinding = AnimatorParamTrack.GetAnimatorFromGameObject((GameObject)playerData);
					_parentBinding = true;
				}
				else if (playerData is Transform)
				{
					_trackBinding = AnimatorParamTrack.GetAnimatorFromGameObject(((Transform)playerData).gameObject);
					_parentBinding = true;
				}
				else
				{
					_parentBinding = false;
				}
			}
			#endregion

			private static int GetHash(string id)
			{
				if (string.IsNullOrEmpty(id))
					return -1;

				return Animator.StringToHash(id);
			}
		}
	}
}
