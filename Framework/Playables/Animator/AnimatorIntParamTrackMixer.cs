using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class AnimatorIntParamTrackMixer : AnimatorParamTrackMixer<int>
		{		
			protected override object GetValue()
			{
				return _trackBinding.GetInteger(_parameterHash);
			}

			protected override object ApplyValue(object value, float inputWeight, Playable playable)
			{
				ScriptPlayable<AnimatorIntParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorIntParamPlayableBehaviour>)playable;
				AnimatorIntParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				value = Mathf.RoundToInt(Mathf.Lerp((int)value, inputBehaviour._value, inputWeight));
				return value;
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetInteger(_parameterHash, (int)value);
			}
		}
	}
}
