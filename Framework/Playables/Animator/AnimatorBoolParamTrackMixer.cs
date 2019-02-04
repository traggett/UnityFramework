using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class AnimatorBoolParamTrackMixer : AnimatorParamTrackMixer<bool>
		{		
			protected override object GetValue()
			{
				return _trackBinding.GetBool(_parameterHash);
			}

			protected override object ApplyValue(object value, float inputWeight, Playable playable)
			{
				ScriptPlayable<AnimatorBoolParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorBoolParamPlayableBehaviour>)playable;
				AnimatorBoolParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				value = inputWeight > 0.5f ? inputBehaviour._value : value;
				return value;
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetBool(_parameterHash, (bool)value);
			}
		}
	}
}
