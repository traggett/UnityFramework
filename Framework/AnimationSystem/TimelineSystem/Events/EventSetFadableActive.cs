using UnityEngine;
using System;

namespace Framework
{
	using Utils;
	using TimelineSystem;

	namespace AnimationSystem
	{
		namespace TimelineSystem
		{
			[Serializable]
			[EventCategory("Animation")]
			public class EventSetFadableActive : Event
			{
				#region Public Data
				public ComponentRef<IFadable> _target;
				public bool _active = false;
				public float _fadeTime = 0.0f;
				#endregion

				#region Event
				public override void Trigger()
				{
					IFadable target = _target.GetComponent();

					if (target != null)
					{
						if (_active)
							target.FadeIn(_fadeTime);
						else
							target.FadeOut(_fadeTime);
					}
				}

				public override float GetDuration()
				{
					return _fadeTime;
				}

#if UNITY_EDITOR
				public override Color GetEditorColor()
				{
					return new Color(0.3f, 0.6f, 0.8f);
				}

				public override string GetEditorDescription()
				{
					if (_active)
						return "Enable " + _target + " over " + _fadeTime + " secs";
					else
						return "Disable " + _target + " over " + _fadeTime + " secs";
				}
#endif
				#endregion
			}
		}
	}
}