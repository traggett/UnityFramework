using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
	[Serializable]
	public class TimelineTickHandler
	{
		private float[] m_tickModulos = new float[0];
		private float[] m_tickStrengths = new float[0];
		private int m_SmallestTick;
		private int m_BiggestTick = -1;
		private float m_MinValue;
		private float m_MaxValue = 1f;
		private float m_PixelRange = 1f;
		public int tickLevels
		{
			get
			{
				return this.m_BiggestTick - this.m_SmallestTick + 1;
			}
		}
		public void SetTickModulos(float[] tickModulos)
		{
			this.m_tickModulos = tickModulos;
		}
		public void SetRanges(float minValue, float maxValue, float minPixel, float maxPixel)
		{
			this.m_MinValue = minValue;
			this.m_MaxValue = maxValue;
			this.m_PixelRange = maxPixel - minPixel;
		}
		public float[] GetTicksAtLevel(int level, bool excludeTicksFromHigherlevels)
		{
			int num = Mathf.Clamp(this.m_SmallestTick + level, 0, this.m_tickModulos.Length - 1);
			List<float> list = new List<float>();
			int num2 = Mathf.FloorToInt(this.m_MinValue / this.m_tickModulos[num]);
			int num3 = Mathf.CeilToInt(this.m_MaxValue / this.m_tickModulos[num]);
			for (int i = num2; i <= num3; i++)
			{
				if (!excludeTicksFromHigherlevels || num >= this.m_BiggestTick || i % Mathf.RoundToInt(this.m_tickModulos[num + 1] / this.m_tickModulos[num]) != 0)
				{
					list.Add((float)i * this.m_tickModulos[num]);
				}
			}
			return list.ToArray();
		}
		public float GetStrengthOfLevel(int level)
		{
			return this.m_tickStrengths[this.m_SmallestTick + level];
		}
		public float GetPeriodOfLevel(int level)
		{
			return this.m_tickModulos[Mathf.Clamp(this.m_SmallestTick + level, 0, this.m_tickModulos.Length - 1)];
		}
		public int GetLevelWithMinSeparation(float pixelSeparation)
		{
			for (int i = 0; i < this.m_tickModulos.Length; i++)
			{
				float num = this.m_tickModulos[i] * this.m_PixelRange / (this.m_MaxValue - this.m_MinValue);
				if (num >= pixelSeparation)
				{
					return i - this.m_SmallestTick;
				}
			}
			return -1;
		}
		public void SetTickStrengths(float tickMinSpacing, float tickMaxSpacing, bool sqrt)
		{
			this.m_tickStrengths = new float[this.m_tickModulos.Length];
			this.m_SmallestTick = 0;
			this.m_BiggestTick = this.m_tickModulos.Length - 1;
			for (int i = this.m_tickModulos.Length - 1; i >= 0; i--)
			{
				float num = this.m_tickModulos[i] * this.m_PixelRange / (this.m_MaxValue - this.m_MinValue);
				this.m_tickStrengths[i] = (num - tickMinSpacing) / (tickMaxSpacing - tickMinSpacing);
				if (this.m_tickStrengths[i] >= 1f)
				{
					this.m_BiggestTick = i;
				}
				if (num <= tickMinSpacing)
				{
					this.m_SmallestTick = i;
					break;
				}
			}
			for (int j = this.m_SmallestTick; j <= this.m_BiggestTick; j++)
			{
				this.m_tickStrengths[j] = Mathf.Clamp01(this.m_tickStrengths[j]);
				if (sqrt)
				{
					this.m_tickStrengths[j] = Mathf.Sqrt(this.m_tickStrengths[j]);
				}
			}
		}
	}
}