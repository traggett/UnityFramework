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
				if (_parameterHash != -1)
					return _trackBinding.GetInteger(_parameterHash);

				return 0;
			}

			protected override object ApplyPlayableValue(object currentValue, Playable playable, float playableWeight)
			{
				ScriptPlayable<AnimatorIntParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<AnimatorIntParamPlayableBehaviour>)playable;
				AnimatorIntParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				return Mathf.RoundToInt(Mathf.Lerp((int)currentValue, inputBehaviour._value, playableWeight));
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetInteger(_parameterHash, (int)value);
			}
		}
	}
}
