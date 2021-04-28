using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace Playables
	{
		public class MaterialColorParamTrackMixer : MaterialParamTrackMixer<Color>
		{		
			protected override object GetValue()
			{
				if (_parameterHash != -1)
					return _trackBinding.GetColor(_parameterHash);

				return 0f;
			}

			protected override object ApplyPlayableValue(object currentValue, Playable playable, float playableWeight)
			{
				ScriptPlayable<MaterialColorParamPlayableBehaviour> scriptPlayable = (ScriptPlayable<MaterialColorParamPlayableBehaviour>)playable;
				MaterialColorParamPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

				return Color.Lerp((Color)currentValue, inputBehaviour._value, playableWeight);
			}

			protected override void SetValue(object value)
			{
				_trackBinding.SetColor(_parameterHash, (Color)value);
			}
		}
	}
}
