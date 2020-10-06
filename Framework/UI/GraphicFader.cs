using UnityEngine;
using UnityEngine.UI;

namespace Framework
{
	namespace UI
	{
		public class GraphicFader : MonoBehaviour
		{
			public Graphic _graphic;

			private float _lerp;
			private float _lerpSpeed;
			private Color _fromColor;
			private Color _toColor;
			private bool _alphaOnly;

			public void FadeToColor(Color color, float time)
			{
				FadeColor(_graphic.color, color, time);
			}

			public void FadeColor(Color from, Color to, float time)
			{
				this.gameObject.SetActive(true);

				if (time > 0f)
				{
					_lerpSpeed = 1f / time;
					_lerp = 0f;
					_fromColor = from;
					_toColor = to;
					_alphaOnly = false;
					_graphic.color = from;
				}
				else
				{
					_graphic.color = to;
					_lerp = 1f;
				}
			}

			public void FadeToAlpha(float alpha, float time)
			{
				FadeAlpha(_graphic.color.a, alpha, time);
			}
			
			public void FadeAlpha(float from, float to, float time)
			{
				this.gameObject.SetActive(true);

				if (time > 0f)
				{
					_lerpSpeed = 1f / time;
					_lerp = 0f;
					_fromColor = _graphic.color;
					_fromColor.a = from;
					_toColor = _graphic.color;
					_toColor.a = to;
					_alphaOnly = true;
					_graphic.color = _fromColor;
				}
				else
				{
					_graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, to);
					_lerp = 1f;
				}
			}

			#region MonoBehaviour
			private void Update()
			{
				if (_lerp < 1f)
				{
					_lerp += Time.deltaTime * _lerpSpeed;

					if (_lerp >= 1f)
					{
						if (_alphaOnly)
						{
							_graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, _toColor.a);
						}
						else
						{
							_graphic.color = _toColor;
						}
					}
					else
					{
						if (_alphaOnly)
						{
							float alpha = Mathf.Lerp(_fromColor.a, _toColor.a, _lerp);
							_graphic.color = new Color(_graphic.color.r, _graphic.color.g, _graphic.color.b, alpha);
						}
						else
						{
							_graphic.color = Color.Lerp(_fromColor, _toColor, _lerp);
						}
					}
				}
			}
			#endregion
		}
	}
}