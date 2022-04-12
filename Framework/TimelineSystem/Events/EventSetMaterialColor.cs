using UnityEngine;
using System;

namespace Framework
{
	using Utils;

	namespace TimelineSystem
	{
		[Serializable]
		[EventCategory("Flow")]
		public class EventSetMaterialColor : Event
		{
			#region Public Data
			public MaterialRef _material;
			public string _propertyName = "_Color";
			public Color _color = Color.black;
			public float _duration = 0.0f;
			#endregion

			private Color _origColor;


			#region Event
			public override void Trigger()
			{
				Material material = _material;

				if (material != null)
				{
					if (_duration > 0.0f)
					{
						_origColor = material.GetColor(_propertyName);
					}
					else
					{
						material.SetColor(_propertyName, _color);
					}
				}
			}

			public override void Update(float eventTime)
			{
				float lerp = eventTime / _duration;
				Color color = Color.Lerp(_origColor, _color, lerp);
				Material material = _material;
				material.SetColor(_propertyName, color);
			}

			public override void End()
			{
				Material material = _material;
				material.SetColor(_propertyName, _color);
			}

			public override float GetDuration()
			{
				return _duration;
			}

#if UNITY_EDITOR
			public override Color GetEditorColor()
			{
				return new Color(_color.r, _color.g, _color.b);
			}

			public override string GetEditorDescription()
			{
				return "Set Material Color ";
			}
#endif
			#endregion
		}
	}
}
