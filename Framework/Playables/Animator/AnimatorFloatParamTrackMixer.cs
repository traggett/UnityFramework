using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class AnimatorFloatParamTrackMixer : AnimatorParamTrackMixer<float>
		{		
			protected override object GetValue()
			{
				if (_parameterHash != -1)
					return _trackBinding.GetFloat(_parameterHash);

				return 0f;
			}

			protected override object ApplyPlayableValue(object currentValue, Playable playable, float playableWeight)
			{
				ScriptPlayable<AnimatorFloatParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorFloatParamPlayableBehaviour>)playable;
				AnimatorFloatParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				return Mathf.Lerp((float)currentValue, inputBehaviour._value, playableWeight);
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetFloat(_parameterHash, (float)value);
			}
		}
	}
}
