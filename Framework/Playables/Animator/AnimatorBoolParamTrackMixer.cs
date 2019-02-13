using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class AnimatorBoolParamTrackMixer : AnimatorParamTrackMixer<bool>
		{		
			protected override object GetValue()
			{
				if (_parameterHash != -1)
					return _trackBinding.GetBool(_parameterHash);

				return false;
			}

			protected override object ApplyPlayableValue(object currentValue, Playable playable, float playableWeight)
			{
				ScriptPlayable<AnimatorBoolParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorBoolParamPlayableBehaviour>)playable;
				AnimatorBoolParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				return playableWeight > 0.5f ? inputBehaviour._value : currentValue;
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetBool(_parameterHash, (bool)value);
			}
		}
	}
}
