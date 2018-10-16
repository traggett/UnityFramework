using UnityEngine;
using System;
using UnityEditor;

namespace Framework
{
	namespace Editor
	{
		[Serializable]
		public class ZoomableScrollArea
		{
			#region Static Data
			public readonly static float kScrollerHeight = 15.0f;

			private static int repeatButtonHash = "repeatButton".GetHashCode();
			private static int s_MinMaxSliderHash = "MinMaxSlider".GetHashCode();
			private static int scrollControlID;
			private static float nextScrollStepTime = 0f;
			private class MinMaxSliderState
			{
				public float dragStartPos;
				public float dragStartValue;
				public float dragStartSize;
				public float dragStartValuesPerPixel;
				public float dragStartLimit;
				public float dragEndLimit;
				public int whereWeDrag = -1;
			}
			private static MinMaxSliderState s_MinMaxSliderState;
			private static DateTime s_NextScrollStepTime = DateTime.Now;
			private static int kFirstScrollWait = 250;
			private static int kScrollWait = 30;
			private readonly static float kMinScroll = 0.0045f;
			private static float kHalfFloatMax = 17014117331926442990585209174226f;
			private readonly static Rect kInfiniteLimts = new Rect(-kHalfFloatMax, -kHalfFloatMax, float.MaxValue, float.MaxValue);

			#region Default Styles
			private static GUIStyle horizontalMinMaxScrollbarThumb;
			public static GUIStyle HorizontalMinMaxScrollbarThumb
			{
				get
				{
					if (horizontalMinMaxScrollbarThumb == null)
						horizontalMinMaxScrollbarThumb = "horizontalMinMaxScrollbarThumb";

					return horizontalMinMaxScrollbarThumb;
				}
			}
			private static GUIStyle horizontalScrollbarLeftButton;
			public static GUIStyle HorizontalScrollbarLeftButton
			{
				get
				{
					if (horizontalScrollbarLeftButton == null)
						horizontalScrollbarLeftButton = "horizontalScrollbarLeftButton";

					return horizontalScrollbarLeftButton;
				}
			}
			private static GUIStyle horizontalScrollbarRightButton;
			public static GUIStyle HorizontalScrollbarRightButton
			{
				get
				{
					if (horizontalScrollbarRightButton == null)
						horizontalScrollbarRightButton = "horizontalScrollbarRightButton";

					return horizontalScrollbarRightButton;
				}
			}
			private static GUIStyle horizontalScrollbar;
			public static GUIStyle HorizontalScrollbar
			{
				get
				{
					if (horizontalScrollbar == null)
						horizontalScrollbar = "horizontalScrollbar";

					return horizontalScrollbar;
				}
			}
			private static GUIStyle verticalMinMaxScrollbarThumb;
			public static GUIStyle VerticalMinMaxScrollbarThumb
			{
				get
				{
					if (verticalMinMaxScrollbarThumb == null)
						verticalMinMaxScrollbarThumb = "verticalMinMaxScrollbarThumb";

					return verticalMinMaxScrollbarThumb;
				}
			}
			private static GUIStyle verticalScrollbarUpButton;
			public static GUIStyle VerticalScrollbarUpButton
			{
				get
				{
					if (verticalScrollbarUpButton == null)
						verticalScrollbarUpButton = "verticalScrollbarUpButton";

					return verticalScrollbarUpButton;
				}
			}
			private static GUIStyle verticalScrollbarDownButton;
			public static GUIStyle VerticalScrollbarDownButton
			{
				get
				{
					if (verticalScrollbarDownButton == null)
						verticalScrollbarDownButton = "verticalScrollbarDownButton";

					return verticalScrollbarDownButton;
				}
			}
			private static GUIStyle verticalScrollbar;
			public static GUIStyle VerticalScrollbar
			{
				get
				{
					if (verticalScrollbar == null)
						verticalScrollbar = "verticalScrollbar";

					return verticalScrollbar;
				}
			}
			#endregion

			#endregion

			#region Private Data
			private int _horizontalScrollbarID;
			private int _verticalScrollbarID;
			#endregion

			#region Public Interface
			public void ZoomableArea(Rect position, ref Rect visibleArea, Rect contentSize, bool scrollHorizontal, bool scrollVertical)
			{
				ZoomableArea(position, ref visibleArea, contentSize, kInfiniteLimts, scrollHorizontal, scrollVertical);
			}

			public void ZoomableArea(Rect position, ref Rect visibleArea, Rect contentSize, Rect scrollableLimits, bool scrollHorizontal, bool scrollVertical)
			{
				_horizontalScrollbarID = GUIUtility.GetControlID(s_MinMaxSliderHash, FocusType.Passive);
				_verticalScrollbarID = GUIUtility.GetControlID(s_MinMaxSliderHash, FocusType.Passive);

				if (scrollHorizontal)
				{
					float value = visibleArea.x;
					float size = visibleArea.width;
					Rect pos = new Rect(position.x, position.y + position.height - kScrollerHeight, scrollVertical ? position.width - kScrollerHeight : position.width, kScrollerHeight);
					MinMaxScroller(pos, _horizontalScrollbarID, ref value, ref size, contentSize.xMin, contentSize.xMax, scrollableLimits.xMin, scrollableLimits.xMax, true);
					visibleArea.x = value;
					visibleArea.width = size;
				}

				if (scrollVertical)
				{
					float value = visibleArea.y;
					float size = visibleArea.height;
					Rect pos = new Rect(position.x + position.width - kScrollerHeight, position.y, kScrollerHeight, scrollHorizontal ? position.height - kScrollerHeight : position.height);
					MinMaxScroller(pos, _verticalScrollbarID, ref value, ref size, contentSize.yMin, contentSize.yMax, scrollableLimits.yMin, scrollableLimits.yMax, false);
					visibleArea.y = value;
					visibleArea.height = size;
				}
			}

			public void BeginZoomableArea(Rect position, ref Rect visibleArea, Rect contentSize, bool scrollHorizontal, bool scrollVertical)
			{
				BeginZoomableArea(position, ref visibleArea, contentSize, kInfiniteLimts, scrollHorizontal, scrollVertical);
			}

			public void BeginZoomableArea(Rect position, ref Rect visibleArea, Rect contentSize, Rect scrollableLimits, bool scrollHorizontal, bool scrollVertical)
			{
				ZoomableArea(position, ref visibleArea, contentSize, scrollableLimits, scrollHorizontal, scrollVertical);

				Rect contentRect = new Rect(position);// new Rect(position.x - _horizontalPosition * position.width, position.y - _verticalPosition * position.height, position.width, position.height);

				if (scrollHorizontal)
					contentRect.height -= kScrollerHeight;

				if (scrollVertical)
					contentRect.width -= kScrollerHeight;

				GUI.BeginGroup(position);
			}

			public void EndZoomableArea()
			{
				GUI.EndGroup();
			}

			public static void MinMaxScroller(Rect position, int id, ref float value, ref float size, float visualStart, float visualEnd, float startLimit, float endLimit, bool horiz, GUIStyle slider = null, GUIStyle thumb = null, GUIStyle leftButton = null, GUIStyle rightButton = null)
			{
				if (slider == null)
					slider = horiz ? HorizontalScrollbar : VerticalScrollbar;
				if (thumb == null)
					thumb = horiz ? HorizontalMinMaxScrollbarThumb : VerticalMinMaxScrollbarThumb;
				if (leftButton == null)
					leftButton = horiz ? HorizontalScrollbarLeftButton : VerticalScrollbarUpButton;
				if (rightButton == null)
					rightButton = horiz ? HorizontalScrollbarRightButton : VerticalScrollbarDownButton;

				float num;
				if (horiz)
				{
					num = size * 10f / position.width;
				}
				else
				{
					num = size * 10f / position.height;
				}
				Rect position2;
				Rect rect;
				Rect rect2;
				if (horiz)
				{
					position2 = new Rect(position.x + leftButton.fixedWidth, position.y, position.width - leftButton.fixedWidth - rightButton.fixedWidth, position.height);
					rect = new Rect(position.x, position.y, leftButton.fixedWidth, position.height);
					rect2 = new Rect(position.xMax - rightButton.fixedWidth, position.y, rightButton.fixedWidth, position.height);
				}
				else
				{
					position2 = new Rect(position.x, position.y + leftButton.fixedHeight, position.width, position.height - leftButton.fixedHeight - rightButton.fixedHeight);
					rect = new Rect(position.x, position.y, position.width, leftButton.fixedHeight);
					rect2 = new Rect(position.x, position.yMax - rightButton.fixedHeight, position.width, rightButton.fixedHeight);
				}
				float num2 = Mathf.Min(visualStart, value);
				float num3 = Mathf.Max(visualEnd, value + size);
				MinMaxSlider(position2, ref value, ref size, num2, num3, num2, num3, slider, thumb, horiz);
				bool flag = false;
				if (Event.current.type == EventType.MouseUp)
				{
					flag = true;
				}
				if (ScrollerRepeatButton(id, rect, leftButton))
				{
					value -= num * ((visualStart >= visualEnd) ? -1f : 1f);
				}
				if (ScrollerRepeatButton(id, rect2, rightButton))
				{
					value += num * ((visualStart >= visualEnd) ? -1f : 1f);
				}
				if (flag && Event.current.type == EventType.Used)
				{
					scrollControlID = 0;
				}
				if (startLimit < endLimit)
				{
					value = Mathf.Clamp(value, startLimit, endLimit - size);
				}
				else
				{
					value = Mathf.Clamp(value, endLimit, startLimit - size);
				}
			}

			public static void MinMaxSlider(Rect position, ref float value, ref float size, float visualStart, float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, bool horiz)
			{
				DoMinMaxSlider(position, GUIUtility.GetControlID(s_MinMaxSliderHash, FocusType.Passive), ref value, ref size, visualStart, visualEnd, startLimit, endLimit, slider, thumb, horiz);
			}
			#endregion

			#region Private Functions
			private static bool ScrollerRepeatButton(int scrollerID, Rect rect, GUIStyle style)
			{
				bool result = false;
				if (DoRepeatButton(rect, GUIContent.none, style, FocusType.Passive))
				{
					bool flag = scrollControlID != scrollerID;
					scrollControlID = scrollerID;
					if (flag)
					{
						result = true;
						nextScrollStepTime = Time.realtimeSinceStartup + 0.001f * (float)kFirstScrollWait;
					}
					else
					{
						if (Time.realtimeSinceStartup >= nextScrollStepTime)
						{
							result = true;
							nextScrollStepTime = Time.realtimeSinceStartup + 0.001f * (float)kScrollWait;
						}
					}
					if (Event.current.type == EventType.Repaint)
					{
						HandleUtility.Repaint();
					}
				}
				return result;
			}

			private static bool DoRepeatButton(Rect position, GUIContent content, GUIStyle style, FocusType focusType)
			{
				int controlID = GUIUtility.GetControlID(repeatButtonHash, focusType, position);
				EventType typeForControl = Event.current.GetTypeForControl(controlID);
				if (typeForControl == EventType.MouseDown)
				{
					if (position.Contains(Event.current.mousePosition))
					{
						GUIUtility.hotControl = controlID;
						Event.current.Use();
					}
					return false;
				}
				if (typeForControl != EventType.MouseUp)
				{
					if (typeForControl != EventType.Repaint)
					{
						return false;
					}
					style.Draw(position, content, controlID);
					return controlID == GUIUtility.hotControl && position.Contains(Event.current.mousePosition);
				}
				else
				{
					if (GUIUtility.hotControl == controlID)
					{
						GUIUtility.hotControl = 0;
						Event.current.Use();
						return position.Contains(Event.current.mousePosition);
					}
					return false;
				}
			}

			private static void DoMinMaxSlider(Rect position, int id, ref float value, ref float size, float visualStart, float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, bool horiz)
			{
				Event current = Event.current;
				bool flag = size == 0f;
				float num = Mathf.Min(visualStart, visualEnd);
				float num2 = Mathf.Max(visualStart, visualEnd);
				float num3 = Mathf.Min(startLimit, endLimit);
				float num4 = Mathf.Max(startLimit, endLimit);
				MinMaxSliderState minMaxSliderState = s_MinMaxSliderState;
				if (GUIUtility.hotControl == id && minMaxSliderState != null)
				{
					num = minMaxSliderState.dragStartLimit;
					num3 = minMaxSliderState.dragStartLimit;
					num2 = minMaxSliderState.dragEndLimit;
					num4 = minMaxSliderState.dragEndLimit;
				}
				float num5 = kMinScroll * (num2 - num);
				float num6 = Mathf.Clamp(value, num, num2);
				float num7 = Mathf.Clamp(value + size, num, num2) - num6;
				float num8 = (float)((visualStart <= visualEnd) ? 1 : -1);
				if (slider == null || thumb == null)
				{
					return;
				}
				float num10;
				Rect position2;
				Rect rect;
				Rect rect2;
				float num11;
				if (horiz)
				{
					float num9 = (thumb.fixedWidth == 0f) ? ((float)thumb.padding.horizontal) : thumb.fixedWidth;
					num10 = (position.width - (float)slider.padding.horizontal - num9) / (num2 - num);
					position2 = new Rect((num6 - num) * num10 + position.x + (float)slider.padding.left, position.y + (float)slider.padding.top, num7 * num10 + num9, position.height - (float)slider.padding.vertical);
					rect = new Rect(position2.x, position2.y, (float)thumb.padding.left, position2.height);
					rect2 = new Rect(position2.xMax - (float)thumb.padding.right, position2.y, (float)thumb.padding.right, position2.height);
					num11 = current.mousePosition.x - position.x;
				}
				else
				{
					float num12 = (thumb.fixedHeight == 0f) ? ((float)thumb.padding.vertical) : thumb.fixedHeight;
					num10 = (position.height - (float)slider.padding.vertical - num12) / (num2 - num);
					position2 = new Rect(position.x + (float)slider.padding.left, (num6 - num) * num10 + position.y + (float)slider.padding.top, position.width - (float)slider.padding.horizontal, num7 * num10 + num12);
					rect = new Rect(position2.x, position2.y, position2.width, (float)thumb.padding.top);
					rect2 = new Rect(position2.x, position2.yMax - (float)thumb.padding.bottom, position2.width, (float)thumb.padding.bottom);
					num11 = current.mousePosition.y - position.y;
				}
				switch (current.GetTypeForControl(id))
				{
					case EventType.MouseDown:
						if (!position.Contains(current.mousePosition) || num - num2 == 0f)
						{
							return;
						}
						if (minMaxSliderState == null)
						{
							minMaxSliderState = (s_MinMaxSliderState = new MinMaxSliderState());
						}
						minMaxSliderState.dragStartLimit = startLimit;
						minMaxSliderState.dragEndLimit = endLimit;
						if (position2.Contains(current.mousePosition))
						{
							minMaxSliderState.dragStartPos = num11;
							minMaxSliderState.dragStartValue = value;
							minMaxSliderState.dragStartSize = size;
							minMaxSliderState.dragStartValuesPerPixel = num10;
							if (rect.Contains(current.mousePosition))
							{
								minMaxSliderState.whereWeDrag = 1;
							}
							else
							{
								if (rect2.Contains(current.mousePosition))
								{
									minMaxSliderState.whereWeDrag = 2;
								}
								else
								{
									minMaxSliderState.whereWeDrag = 0;
								}
							}
							GUIUtility.hotControl = id;
							current.Use();
							return;
						}
						if (slider == GUIStyle.none)
						{
							return;
						}
						if (size != 0f && flag)
						{
							if (horiz)
							{
								if (num11 > position2.xMax - position.x)
								{
									value += size * num8 * 0.9f;
								}
								else
								{
									value -= size * num8 * 0.9f;
								}
							}
							else
							{
								if (num11 > position2.yMax - position.y)
								{
									value += size * num8 * 0.9f;
								}
								else
								{
									value -= size * num8 * 0.9f;
								}
							}
							minMaxSliderState.whereWeDrag = 0;
							GUI.changed = true;
							s_NextScrollStepTime = DateTime.Now.AddMilliseconds((double)kFirstScrollWait);
							float num13 = (!horiz) ? current.mousePosition.y : current.mousePosition.x;
							float num14 = (!horiz) ? position2.y : position2.x;
							minMaxSliderState.whereWeDrag = ((num13 <= num14) ? 3 : 4);
						}
						else
						{
							if (horiz)
							{
								value = (num11 - position2.width * 0.5f) / num10 + num - size * 0.5f;
							}
							else
							{
								value = (num11 - position2.height * 0.5f) / num10 + num - size * 0.5f;
							}
							minMaxSliderState.dragStartPos = num11;
							minMaxSliderState.dragStartValue = value;
							minMaxSliderState.dragStartSize = size;
							minMaxSliderState.dragStartValuesPerPixel = num10;
							minMaxSliderState.whereWeDrag = 0;
							GUI.changed = true;
						}
						GUIUtility.hotControl = id;
						value = Mathf.Clamp(value, num3, num4 - size);
						current.Use();
						return;
					case EventType.MouseUp:
						if (GUIUtility.hotControl == id)
						{
							current.Use();
							GUIUtility.hotControl = 0;
						}
						break;
					case EventType.MouseDrag:
						{
							if (GUIUtility.hotControl != id)
							{
								return;
							}
							float num15 = (num11 - minMaxSliderState.dragStartPos) / minMaxSliderState.dragStartValuesPerPixel;
							switch (minMaxSliderState.whereWeDrag)
							{
								case 0:
									value = Mathf.Clamp(minMaxSliderState.dragStartValue + num15, num3, num4 - size);
									break;
								case 1:
									value = minMaxSliderState.dragStartValue + num15;
									size = minMaxSliderState.dragStartSize - num15;
									if (value < num3)
									{
										size -= num3 - value;
										value = num3;
									}
									if (size < num5)
									{
										value -= num5 - size;
										size = num5;
									}
									break;
								case 2:
									size = minMaxSliderState.dragStartSize + num15;
									if (value + size > num4)
									{
										size = num4 - value;
									}
									if (size < num5)
									{
										size = num5;
									}
									break;
							}
							GUI.changed = true;
							current.Use();
							break;
						}
					case EventType.Repaint:
						{
							slider.Draw(position, GUIContent.none, id);
							thumb.Draw(position2, GUIContent.none, id);
							if (GUIUtility.hotControl != id || !position.Contains(current.mousePosition) || num - num2 == 0f)
							{
								return;
							}
							if (position2.Contains(current.mousePosition))
							{
								if (minMaxSliderState != null && (minMaxSliderState.whereWeDrag == 3 || minMaxSliderState.whereWeDrag == 4))
								{
									GUIUtility.hotControl = 0;
								}
								return;
							}
							if (DateTime.Now < s_NextScrollStepTime)
							{
								return;
							}
							float num13 = (!horiz) ? current.mousePosition.y : current.mousePosition.x;
							float num14 = (!horiz) ? position2.y : position2.x;
							int num16 = (num13 <= num14) ? 3 : 4;
							if (num16 != minMaxSliderState.whereWeDrag)
							{
								return;
							}
							if (size != 0f && flag)
							{
								if (horiz)
								{
									if (num11 > position2.xMax - position.x)
									{
										value += size * num8 * 0.9f;
									}
									else
									{
										value -= size * num8 * 0.9f;
									}
								}
								else
								{
									if (num11 > position2.yMax - position.y)
									{
										value += size * num8 * 0.9f;
									}
									else
									{
										value -= size * num8 * 0.9f;
									}
								}
								minMaxSliderState.whereWeDrag = -1;
								GUI.changed = true;
							}
							value = Mathf.Clamp(value, num3, num4 - size);
							s_NextScrollStepTime = DateTime.Now.AddMilliseconds((double)kScrollWait);
							break;
						}
				}
			}
			#endregion
		}
	}
}