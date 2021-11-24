using UnityEngine;

namespace Framework
{
    using Maths;
    
    namespace Utils
    {
		public abstract class ColorFader : MonoBehaviour
		{
			private Color _from;
			private Color _target;
			private bool _alphaOnly;
			private float _lerp;
			private float _lerpSpeed;

			public void SetAlpha(float targetAlpha, float time = 0f)
			{
				if (time <= 0f)
				{
					OnSetAlpha(targetAlpha);
					_lerp = 0f;
				}
				else
				{
					if (_lerp <= 0f || !MathUtils.Approximately(targetAlpha, _target.a, Mathf.Epsilon))
					{
						_lerp = 1f;
						_from = GetColor();
					}

					_alphaOnly = true;
					_lerpSpeed = 1f / time;
					_target = new Color(0f, 0f, 0f, targetAlpha);
				}			
			}

			public void SetColor(Color target, float time = 0f)
			{
				if (time <= 0f)
				{
					OnSetColor(target);
					_lerp = 0f;
				}
				else
				{
					if (_lerp <= 0f || !ColorUtils.Approximately(target, _target, Mathf.Epsilon))
					{
						_lerp = 1f;
						_from = GetColor();
					}

					_alphaOnly = false;
					_lerpSpeed = 1f / time;
					_target = target;
				}
			}

			protected abstract Color GetColor();
			protected abstract void OnSetColor(Color color);
			protected abstract void OnSetAlpha(float alpha);

			#region Unity Messages
			private void Update()
			{
				if (_lerp > 0f)
				{
					_lerp -= _lerpSpeed * Time.deltaTime;

					if (_lerp < 0f)
					{
						if (_alphaOnly)
						{
							OnSetAlpha(_target.a);
						}
						else
						{
							OnSetColor(_target);
						}
					}
					else
					{
						if (_alphaOnly)
						{
							OnSetAlpha(Mathf.Lerp(_target.a, _from.a, _lerp));
						}
						else
						{
							OnSetColor(Color.Lerp(_target, _from, _lerp));
						}
					}
				}
			}
			#endregion
		}
	}
}