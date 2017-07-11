using UnityEngine;

using System;
using System.Reflection;
using UnityEditor;

namespace Framework
{
	[Serializable]
	public class TimelineScrollArea
	{
		#region Public Data
		public enum eTimeFormat
		{
			Default,
			DaysHoursMins,
		}

		public float PlayHeadTime
		{
			set
			{
				_playHeadTime = value;
			}
			get
			{
				return _playHeadTime;
			}
		}

		public bool NeedsRepaint
		{
			set
			{
				_needsRepaint = value;
			}
			get
			{
				return _needsRepaint;
			}
		}
		#endregion

		#region Static Data
		private static MethodInfo handleUtilityApplyWireMaterial_MethodInfo;
		private static GUIStyle _rulerStyle;
		private static GUIStyle _tickLabelStyle;
		private static GUIStyle _spacerStyle;
		private static GUIStyle _backgroundStyle;
		private static readonly float kTimeRulerSize = 18.0f;
		#endregion

		#region Private Data
		private ZoomableScrollArea _scrollArea;
		private TimelineTickHandler _ticks;
		private float _playHeadTime;
		private eTimeFormat _timeFormat;
		private Rect _position;
		private Rect _contentAreaRect;
		private Rect _visibleTimeRect;
		private bool _needsRepaint;
		#endregion

		#region Public Interface
		public TimelineScrollArea(eTimeFormat timeFormat)
		{
			_scrollArea = new ZoomableScrollArea();
			_ticks = new TimelineTickHandler();

			float[] tickModulos = null;

			_timeFormat = timeFormat;

			switch (_timeFormat)
			{
				case eTimeFormat.Default:
					{
						tickModulos = new float[]
						{
						0.125f,
						0.25f,
						0.5f,
						1.0f,
						5.0f,
						10.0f,
						15.0f,
						30.0f,
						60.0f,
						60.0f * 5.0f,
						60.0f * 10.0f,
						};
					}
					break;
				case eTimeFormat.DaysHoursMins:
					{
						tickModulos = new float[]
						{
						30.0f,
						60.0f,
						60.0f * 5.0f,
						60.0f * 10.0f,
						60.0f * 30.0f,
						60.0f * 60.0f,
						60.0f * 60.0f * 24.0f,
						};
					}
					break;
			}

			_ticks.SetTickModulos(tickModulos);
		}

		public Rect BeginTimedArea(Rect position, float contentStartTime, float contentEndTime, ref float visibleStartTime, ref float visibleDuration, float contentStartPadding = 0.0f, float contentEndPadding = 0.0f, float minScrollableTime = 0.0f, float maxScrollableTime = float.MaxValue, bool allowRenderOverSpacer = false)
		{
			_position = position;
			_needsRepaint = false;

			if (_rulerStyle == null)
			{
				_rulerStyle = "TE Toolbar";
				_tickLabelStyle = "AnimationTimelineTick";
				_spacerStyle = "AnimationEventBackground";
				_backgroundStyle = "flow background";
			}

			//Work out if outside of content range is viewable on either side
			bool startOfContentVisible = _visibleTimeRect.xMin < contentStartTime;
			float startOfContentViewFraction = 0.0f;
			if (startOfContentVisible)
				startOfContentViewFraction = Mathf.Clamp01((contentStartTime - _visibleTimeRect.xMin) / _visibleTimeRect.width);
			bool endOfContentVisible = contentEndTime < _visibleTimeRect.xMax;
			float endOfContentViewFraction = 1.0f;
			if (endOfContentVisible)
				endOfContentViewFraction = Mathf.Clamp01((contentEndTime - _visibleTimeRect.xMin) / _visibleTimeRect.width);

			//Draw ruler
			Rect rulerRect = new Rect(position.x, position.y, position.width, _rulerStyle.fixedHeight);
			{
				GUI.Label(rulerRect, string.Empty, _rulerStyle);

				if (startOfContentVisible)
					DrawDisabledOverlay(new Rect(position.x, rulerRect.y, position.width * startOfContentViewFraction, rulerRect.height), _rulerStyle, 0.15f);
				if (endOfContentVisible)
					DrawDisabledOverlay(new Rect(position.x + position.width * endOfContentViewFraction, rulerRect.y, position.width * (1.0f - endOfContentViewFraction), rulerRect.height), _rulerStyle, 0.15f);

				DrawRuler(rulerRect);

				HandleRulerInput(rulerRect);
			}

			//Draw spacer
			Rect spacerRect = new Rect(position.x, rulerRect.y + rulerRect.height, position.width, kTimeRulerSize);
			GUI.Label(spacerRect, GUIContent.none, _spacerStyle);

			//Draw timed area
			Rect timedAreaRect = new Rect(position.x, spacerRect.y + spacerRect.height, position.width, position.height - spacerRect.height - rulerRect.height - ZoomableScrollArea.kScrollerHeight);
			{
				GUI.Label(timedAreaRect, GUIContent.none, _backgroundStyle);

				if (startOfContentVisible)
					DrawDisabledOverlay(new Rect(timedAreaRect.x, timedAreaRect.y, timedAreaRect.x + timedAreaRect.width * startOfContentViewFraction, timedAreaRect.height), _backgroundStyle, 0.25f);

				if (endOfContentVisible)
					DrawDisabledOverlay(new Rect(timedAreaRect.x + timedAreaRect.width * endOfContentViewFraction, timedAreaRect.y, timedAreaRect.width * (1.0f - endOfContentViewFraction), timedAreaRect.height), _backgroundStyle, 0.25f);

				//Draw tick lines over background
				DrawTickLines(timedAreaRect);
			}

			//Draw content border lines
			{
				Color contentBorders = new Color(0.5f, 0.5f, 0.5f, 1.0f);
				//Draw content start line
				DrawLineAtTime(contentStartTime, contentBorders);
				//Draw content end line
				DrawLineAtTime(contentEndTime, contentBorders);
			}

			//ZoomableArea
			{
				_visibleTimeRect = new Rect(visibleStartTime, 0.0f, visibleDuration, 0.0f);

				Rect contentRect = new Rect();
				contentRect.xMin = contentStartTime - contentStartPadding;
				contentRect.xMax = contentEndTime + contentEndPadding;

				Rect scrollableLimits = new Rect(minScrollableTime, 0.0f, maxScrollableTime, 0.0f);

				Rect zoomableAreaRect = new Rect(position.x, timedAreaRect.y, position.width, timedAreaRect.height + ZoomableScrollArea.kScrollerHeight);
				_scrollArea.ZoomableArea(zoomableAreaRect, ref _visibleTimeRect, contentRect, scrollableLimits, true, false);

				visibleStartTime = _visibleTimeRect.x;
				visibleDuration = _visibleTimeRect.width;
			}


			_contentAreaRect = new Rect(position.x * 0.5f, timedAreaRect.y, position.width, position.height - spacerRect.height - rulerRect.height - ZoomableScrollArea.kScrollerHeight);
			if (allowRenderOverSpacer)
			{
				_contentAreaRect.y -= kTimeRulerSize;
				_contentAreaRect.height += kTimeRulerSize;
			}

			GUI.BeginGroup(_contentAreaRect);

			return _contentAreaRect;
		}

		private void DrawDisabledOverlay(Rect rect, GUIStyle style, float alpha)
		{
			Color origCol = GUI.color;
			GUI.color = new Color(0.0f, 0.0f, 0.0f, alpha);
			GUI.Label(rect, GUIContent.none, style);
			GUI.color = origCol;
		}

		private void HandleRulerInput(Rect rect)
		{
			switch (Event.current.type)
			{
				case EventType.MouseDown:
					{
						if (rect.Contains(Event.current.mousePosition))
						{
							float time = GetTime(Event.current.mousePosition.x - rect.x * 0.5f);
							PlayHeadTime = time;
							_needsRepaint = true;
						}
					}
					break;
			}
		}

		public void EndTimedArea()
		{
			GUI.EndGroup();

			Color playHeadColor = Color.red;

			//Draw playhead
			DrawLineAtTime(_playHeadTime, playHeadColor);
		}

		public float GetPosition(float time)
		{
			float viewFraction = (time - _visibleTimeRect.xMin) / _visibleTimeRect.width;
			return _contentAreaRect.xMin + viewFraction * _contentAreaRect.width;
		}

		public float GetTime(float positon)
		{
			float viewFraction = (positon - _contentAreaRect.xMin) / _contentAreaRect.width;
			return _visibleTimeRect.xMin + viewFraction * _visibleTimeRect.width;
		}

		public float GetTimeDelta(float positonDelta)
		{
			return positonDelta * _visibleTimeRect.width / _contentAreaRect.width;
		}

		public float GetPositonDelta(float timeDelta)
		{
			return timeDelta * _contentAreaRect.width / _visibleTimeRect.width;
		}

		public bool IsTimeVisible(float time)
		{
			return _visibleTimeRect.xMin <= time && time < _visibleTimeRect.xMax;
		}

		public eTimeFormat GetTimeFormat()
		{
			return _timeFormat;
		}
		#endregion

		#region Private Functions
		private static void PrepareForLineDrawing()
		{
			if (handleUtilityApplyWireMaterial_MethodInfo == null)
			{
				BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
				handleUtilityApplyWireMaterial_MethodInfo = typeof(HandleUtility).GetMethod("ApplyWireMaterial", bindingFlags, null, new Type[] { }, null);
			}

			if (handleUtilityApplyWireMaterial_MethodInfo != null)
			{
				handleUtilityApplyWireMaterial_MethodInfo.Invoke(null, null);
			}
		}

		private static void DrawLine(Vector2 from, Vector2 to, Color color)
		{
			PrepareForLineDrawing();
			GL.Begin(GL.LINES);
			GL.Color(color);
			GL.Vertex(from);
			GL.Vertex(to);
			GL.End();
		}

		private void DrawRuler(Rect position)
		{
			Color color = GUI.color;
			GUI.BeginGroup(position);
			if (Event.current.type != EventType.Repaint)
			{
				GUI.EndGroup();
				return;
			}

			PrepareForLineDrawing();
			GL.Begin(GL.LINES);

			Color backgroundColor = GUI.backgroundColor;
			_ticks.SetRanges(_visibleTimeRect.xMin, _visibleTimeRect.xMax, position.x, position.x + position.width);
			_ticks.SetTickStrengths(3f, 80f, true);

			Color textColor = _tickLabelStyle.normal.textColor;
			textColor.a = 0.4f;

			for (int i = 0; i < _ticks.tickLevels; i++)
			{
				float strength = _ticks.GetStrengthOfLevel(i) * 0.9f;
				float[] ticksAtLevel = _ticks.GetTicksAtLevel(i, true);
				for (int j = 0; j < ticksAtLevel.Length; j++)
				{
					if (ticksAtLevel[j] >= _visibleTimeRect.xMin && ticksAtLevel[j] <= _visibleTimeRect.xMax)
					{
						float y = position.height * Mathf.Min(1f, strength) * 0.7f;
						float xPos = Mathf.Floor(TimeToPixel(ticksAtLevel[j], position)) + 0.5f;

						GL.Color(new Color(1f, 1f, 1f, strength / 0.5f) * textColor);
						GL.Vertex(new Vector3(xPos, position.height - y + 0.5f, 0f));
						GL.Vertex(new Vector3(xPos, position.height - 0.5f, 0f));
						if (strength > 0.5f)
						{
							GL.Color(new Color(1f, 1f, 1f, strength / 0.5f - 1f) * textColor);
							GL.Vertex(new Vector3(xPos, position.height - y + 0.5f, 0f));
							GL.Vertex(new Vector3(xPos, position.height - 0.5f, 0f));
						}
					}
				}
			}
			GL.End();

			int levelWithMinSeparation = Math.Max(_ticks.GetLevelWithMinSeparation(40f), _timeFormat == eTimeFormat.Default ? 3 : 1);
			float[] ticksAtLevel2 = _ticks.GetTicksAtLevel(levelWithMinSeparation, false);
			for (int k = 0; k < ticksAtLevel2.Length; k++)
			{
				if (ticksAtLevel2[k] >= _visibleTimeRect.xMin && ticksAtLevel2[k] <= _visibleTimeRect.xMax)
				{
					float xPos = TimeToPixel(ticksAtLevel2[k], position);
					string text = GetTimeString(ticksAtLevel2[k]);
					GUI.Label(new Rect(xPos + 3f, -3f, 40f, 20f), text, _tickLabelStyle);
				}
			}
			GUI.EndGroup();
			GUI.backgroundColor = backgroundColor;
			GUI.color = color;
		}

		private void DrawTickLines(Rect position)
		{
			Color color = Handles.color;
			GUI.BeginGroup(position);
			if (Event.current.type != EventType.Repaint)
			{
				GUI.EndGroup();
				return;
			}

			PrepareForLineDrawing();
			GL.Begin(GL.LINES);

			Color backgroundColor = GUI.backgroundColor;
			_ticks.SetRanges(_visibleTimeRect.xMin, _visibleTimeRect.xMax, position.x, position.x + position.width);
			_ticks.SetTickStrengths(3f, 80f, true);

			Color textColor = _tickLabelStyle.normal.textColor;
			textColor.a = 0.2f;

			for (int i = 0; i < _ticks.tickLevels; i++)
			{
				float strength = _ticks.GetStrengthOfLevel(i) * 0.9f;
				if (strength > 0.25f)
				{
					float[] ticksAtLevel = _ticks.GetTicksAtLevel(i, true);
					for (int j = 0; j < ticksAtLevel.Length; j++)
					{
						float x = Mathf.Floor(TimeToPixel(ticksAtLevel[j], position)) + 0.5f;

						GL.Color(new Color(1f, 1f, 1f, strength / 0.5f) * textColor);
						GL.Vertex(new Vector3(x, 0f, 0f));
						GL.Vertex(new Vector3(x, position.height, 0f));
						if (strength > 0.5f)
						{
							GL.Color(new Color(1f, 1f, 1f, strength / 0.5f - 1f) * textColor);
							GL.Vertex(new Vector3(x, 0.0f, 0f));
							GL.Vertex(new Vector3(x, position.height, 0f));
						}
					}
				}
			}
			GL.End();

			GUI.EndGroup();
		}

		private void DrawLineAtTime(float time, Color color)
		{
			float viewFraction = ((time - _visibleTimeRect.xMin) / _visibleTimeRect.width);
			if (0.0f <= viewFraction && viewFraction < 1.0f)
			{
				float xPos = Mathf.Floor(_position.x + viewFraction * _position.width) + 0.5f;
				DrawLine(new Vector2(xPos, _position.yMin + 0.5f), new Vector2(xPos, _position.yMax - ZoomableScrollArea.kScrollerHeight + 0.5f), color);
			}
		}

		private float TimeToPixel(float time, Rect rect)
		{
			return (time - _visibleTimeRect.xMin) * rect.width / _visibleTimeRect.width;
		}

		private string GetTimeString(float time)
		{
			string label = "";

			if (time < 0.0f)
			{
				label = "-";
				time = Mathf.Abs(time);
			}

			switch (_timeFormat)
			{
				case eTimeFormat.Default:
					{
						float seconds = time % 60.0f;
						float minutes = (time - seconds) / 60.0f;
						label += (minutes < 10 ? "0" : string.Empty) + Convert.ToString(minutes) + ':' + (seconds < 10 ? "0" : string.Empty) + Convert.ToString(seconds);
					}
					break;
				case eTimeFormat.DaysHoursMins:
					{
						float minutes = Mathf.Round(time / 60.0f);
						float hours = Mathf.Floor(minutes / 60.0f);
						float days = Mathf.Floor(hours / 24.0f);
						
						minutes = minutes % 60.0f;
						hours = hours % 24.0f;

						if (days >= 1.0f)
							label += Convert.ToString(days) + 'd';

						label += (hours < 10 ? "0" : string.Empty) + Convert.ToString(hours) + 'h' + (minutes < 10 ? "0" : string.Empty) + Convert.ToString(minutes);
					}
					break;
			}

			return label;
		}
		#endregion
	}
}