using UnityEngine;

namespace Framework
{
	namespace Utils
	{
		public struct HSLColor
		{
			public float h;
			public float s;
			public float l;
			public float a;


			public HSLColor(float h, float s, float l, float a)
			{
				this.h = h;
				this.s = s;
				this.l = l;
				this.a = a;
			}

			public HSLColor(float h, float s, float l)
			{
				this.h = h;
				this.s = s;
				this.l = l;
				this.a = 1f;
			}

			public HSLColor(Color c)
			{
				HSLColor temp = FromRGBA(c);
				h = temp.h;
				s = temp.s;
				l = temp.l;
				a = temp.a;
			}

			public static HSLColor FromRGBA(Color c)
			{
				float h, s, l, a;
				a = c.a;

				float cmin = Mathf.Min(Mathf.Min(c.r, c.g), c.b);
				float cmax = Mathf.Max(Mathf.Max(c.r, c.g), c.b);

				l = (cmin + cmax) / 2f;

				if (cmin == cmax)
				{
					s = 0;
					h = 0;
				}
				else
				{
					float delta = cmax - cmin;

					s = (l <= .5f) ? (delta / (cmax + cmin)) : (delta / (2f - (cmax + cmin)));

					h = 0;

					if (c.r == cmax)
					{
						h = (c.g - c.b) / delta;
					}
					else if (c.g == cmax)
					{
						h = 2f + (c.b - c.r) / delta;
					}
					else if (c.b == cmax)
					{
						h = 4f + (c.r - c.g) / delta;
					}

					h = Mathf.Repeat(h * 60f, 360f);
				}

				return new HSLColor(h, s, l, a);
			}


			public Color ToRGBA()
			{
				float r, g, b, a;
				a = this.a;

				float m1, m2;

				//	Note: there is a typo in the 2nd International Edition of Foley and
				//	van Dam's "Computer Graphics: Principles and Practice", section 13.3.5
				//	(The HLS Color Model). This incorrectly replaces the 1f in the following
				//	line with "l", giving confusing results.
				m2 = (l <= .5f) ? (l * (1f + s)) : (l + s - l * s);
				m1 = 2f * l - m2;

				if (s == 0f)
				{
					r = g = b = l;
				}
				else
				{
					r = Value(m1, m2, h + 120f);
					g = Value(m1, m2, h);
					b = Value(m1, m2, h - 120f);
				}

				return new Color(r, g, b, a);
			}

			static float Value(float n1, float n2, float hue)
			{
				hue = Mathf.Repeat(hue, 360f);

				if (hue < 60f)
				{
					return n1 + (n2 - n1) * hue / 60f;
				}
				else if (hue < 180f)
				{
					return n2;
				}
				else if (hue < 240f)
				{
					return n1 + (n2 - n1) * (240f - hue) / 60f;
				}
				else
				{
					return n1;
				}
			}

			public static implicit operator HSLColor(Color src)
			{
				return FromRGBA(src);
			}

			public static implicit operator Color(HSLColor src)
			{
				return src.ToRGBA();
			}

		}
	}
}