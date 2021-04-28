using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class MaterialFloatParamTrackMixer : MaterialParamTrackMixer<float>
		{		
			protected override object GetValue()
			{
				if (_parameterHash != -1)
					return _trackBinding.GetFloat(_parameterHash);

				return 0f;
			}

			protected override object ApplyPlayableValue(object currentValue, Playable playable, float playableWeight)
			{
				ScriptPlayable<MaterialFloatParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<MaterialFloatParamPlayableBehaviour>)playable;
				MaterialFloatParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				return Mathf.Lerp((float)currentValue, inputBehaviour._value, playableWeight);
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetFloat(_parameterHash, (float)value);
			}
		}
	}
}
