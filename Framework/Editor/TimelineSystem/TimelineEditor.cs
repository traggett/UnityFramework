using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace Framework
{
	using Utils;
	using Serialization;
	using Editor;

	namespace TimelineSystem
	{
		namespace Editor
		{
			public class TimelineEditor : SerializedObjectEditor<Event>
			{
				public interface IEditor : IEditorWindow
				{
					void OnAddedNewObjectToTimeline(object obj);
#if DEBUG
					bool IsDebugging();
#endif
				}
				
				public TimelineEditor()
				{

				}

				#region Private Data
				private TimelineScrollArea _timelineArea;
				private float _shownStartTime;
				private float _shownDuration;
				private float _visiblePadding;
				
				private Type[] _allowedEventTypes;
				private static Dictionary<Type, string> _eventCategory;
				private static readonly float kArrowHeight = 6.0f;
				private static readonly float kArrowWidth = 4.0f;
				private static readonly float kPinWidth = 2.4f;
				#endregion

				#region Public Methods
				public void Init(IEditor parent, TimelineScrollArea.eTimeFormat timeFormat)
				{
					_timelineArea = new TimelineScrollArea(timeFormat);
					_visiblePadding = timeFormat == TimelineScrollArea.eTimeFormat.Default ? 5.0f : 60.0f * 60.0f;
					SetEditorWindow(parent);
				}
				
				public float GetPlayModeCursorTime()
				{
					return _timelineArea.PlayHeadTime;
				}

				public void SetPlayModeCursorTime(float time)
				{
					_timelineArea.PlayHeadTime = time;
					SetNeedsRepaint();
				}

				public void SetEventTypes(Type[] eventTypes)
				{
					_allowedEventTypes = eventTypes;
				}

				public Type[] GetAllowedEventTypes()
				{
					return _allowedEventTypes;
				}

				public void SetTimeline(Timeline timeline)
				{
					ClearObjects();
					
					if (timeline != null)
					{
						foreach (Event evnt in timeline._events)
						{
							AddNewObject(evnt);
						}
					}
					
					_shownStartTime = 0.0f;
					_shownDuration = GetTimelineEndTime() + _visiblePadding;
					_timelineArea.PlayHeadTime = 0.0f;
				}

				public Timeline ConvertToTimeline()
				{
					SortObjects();

					Timeline timeLine = new Timeline();

					Event[] events = new Event[_editableObjects.Count];
					for (int i = 0; i < _editableObjects.Count; i++)
					{
						events[i] = ((EventEditorGUI)_editableObjects[i]).GetEditableObject();
					}
					timeLine._events = events;

					return timeLine;
				}
				
				public void Render(Rect position, GUIStyle style)
				{
					//Start main container
					_timelineArea.BeginTimedArea(position, 0.0f, GetTimelineEndTime(), ref _shownStartTime, ref _shownDuration, 0.0f, _visiblePadding, 0.0f, float.MaxValue, true);
					{
						SetNeedsRepaint(_timelineArea.NeedsRepaint);

						// Render Events
						{
							List<Rect> eventRects = new List<Rect>();

							foreach (EventEditorGUI evnt in _editableObjects)
							{
								float xPos = Mathf.Floor(_timelineArea.GetPosition(evnt.GetTime()));
								float yPos =  20.0f;
								float eventSize = _timelineArea.GetPositonDelta(evnt.GetDuration());
								Vector2 pos = new Vector2(xPos, yPos);

								evnt.CalcBounds(pos, eventSize, style);
								Rect eventRect = evnt.GetBounds();
								
								bool overlapping = true;

								while (overlapping)
								{
									overlapping = false;

									//Check overlaps prev events, if so shift down y
									foreach (Rect otherEventRect in eventRects)
									{
										if (eventRect.Overlaps(otherEventRect))
										{
											overlapping = true;
											eventRect.y = otherEventRect.yMax;
											break;
										}
									}
								}
								
								evnt.SetBounds(eventRect);
								eventRects.Add(new Rect(eventRect.x, eventRect.y, eventRect.width + 4.0f, eventRect.height + 4.0f));

								RenderEventArrow(evnt, false);
							}

							foreach (EventEditorGUI evnt in _editableObjects)
							{
								float eventSize = _timelineArea.GetPositonDelta(evnt.GetDuration());
								bool selected = _selectedObjects.Contains(evnt);
								evnt.RenderOnTimeline(style, eventSize, selected);

								if (selected)
								{
									RenderEventArrow(evnt, true);
								}
							}
						}

						//Input
						HandleInput();						
					}
					_timelineArea.EndTimedArea();
				}
				
				public IEditor GetParent()
				{
					return (IEditor)GetEditorWindow();
				}

				public void SetEventTime(EventEditorGUI evnt, float time)
				{
					time = Mathf.Max(time, 0.0f);

					if (evnt.EndsTimeline())
					{
						foreach (EventEditorGUI evt in _editableObjects)
						{
							if (!evt.EndsTimeline())
								time = Math.Max(time, evt.GetTime() + evt.GetDuration());
						}
					}

					evnt.SetTime(time);
					evnt.MarkAsDirty(true);
				}

				public TimelineScrollArea.eTimeFormat GetTimeFormat()
				{
					return _timelineArea.GetTimeFormat();
				}
				#endregion

				#region EditableObjectEditor
				protected override SerializedObjectEditorGUI<Event> CreateObjectEditorGUI(Event evnt)
				{
					return EventEditorGUI.CreateEventEditorGUI(this, evnt);
				}

				protected override void OnCreatedNewObject(Event evnt)
				{
					GetParent().OnAddedNewObjectToTimeline(evnt);
				}

				protected override Event CreateCopyFrom(SerializedObjectEditorGUI<Event> editorGUI)
				{
					return Serializer.CreateCopy(editorGUI.GetEditableObject());
				}

				protected override void SetObjectPosition(SerializedObjectEditorGUI<Event> editorGUI, Vector2 position)
				{
					SetEventTime((EventEditorGUI)editorGUI, _timelineArea.GetTime(position.x));
				}

				protected override void AddContextMenu(GenericMenu menu)
				{
					foreach (Type type in GetAllowedEventTypes())
					{
						string category = GetCategory(type);

						if (category == EventCategoryAttribute.kCoreEvent)
						{
							menu.AddItem(new GUIContent(GetEventMenuName(type)), false, AddNewEventMenuCallback, type);
						}
					}

					menu.AddSeparator(string.Empty);

					foreach (Type type in GetAllowedEventTypes())
					{
						string category = GetCategory(type);

						if (category != EventCategoryAttribute.kCoreEvent)
						{
							string menuItemName = "Add Event/";
							menu.AddItem(new GUIContent(menuItemName + category + "/" + GetEventMenuName(type)), false, AddNewEventMenuCallback, type);
						}
					}
				}
				
				protected override void DragObjects(Vector2 delta)
				{
					float timeDelta = _timelineArea.GetTimeDelta(delta.x);
					
					if (timeDelta < 0.0f)
					{
						foreach (EventEditorGUI evnt in _selectedObjects)
						{
							timeDelta = Mathf.Max(-evnt.GetTime(), timeDelta);
						}
					}

					foreach (EventEditorGUI evnt in _selectedObjects)
					{
						SetEventTime(evnt, Mathf.Max(evnt.GetTime() + timeDelta, 0.0f));
					}
				}

				protected override void ScrollEditorView(Vector2 delta)
				{
					
				}

				protected virtual void OnZoomChanged(float zoom)
				{

				}

				protected override void ZoomEditorView(float amount)
				{

				}
				#endregion

				#region Private Methods
				private float GetTimelineEndTime()
				{
					float endTime = 0.0f;

					foreach (EventEditorGUI evnt in _editableObjects)
					{
						endTime = Mathf.Max(endTime, evnt.GetTime() + evnt.GetDuration());
					}

					return endTime;
				}

				private float GetActualTimelineEndTime(float maxShown)
				{
					float maxTime = maxShown;

					foreach (EventEditorGUI ent in _editableObjects)
					{
						if (ent.EndsTimeline())
						{
							maxTime = Math.Min(maxTime, ent.GetTime());
						}
					}

					return maxTime;
				}

				private void RenderEventArrow(EventEditorGUI evnt, bool selected)
				{
					if (_timelineArea.IsTimeVisible(evnt.GetTime()))
					{
						float position = _timelineArea.GetPosition(evnt.GetTime());					
						float xPos = Mathf.Floor(position) + 0.5f;

						Handles.BeginGUI();

						//Draw arrow head / pin
						{
							if (selected)
							{
								Handles.color = Color.white;

								Handles.DrawSolidDisc(new Vector3(xPos, evnt.GetBounds().yMax + kPinWidth, 0.0f), -Vector3.forward, 4.24f);

								Handles.DrawAAConvexPolygon(new Vector3(xPos, 0f, 0.0f),
								new Vector3(xPos + kArrowWidth + 2f, 2f + kArrowHeight, 0.0f),
								new Vector3(xPos - kArrowWidth - 2f, 2f + kArrowHeight, 0.0f));

								Handles.color = evnt.GetColor();

								Handles.DrawSolidDisc(new Vector3(xPos, evnt.GetBounds().yMax + kPinWidth, 0.0f), -Vector3.forward, kPinWidth);

								Handles.DrawAAConvexPolygon(new Vector3(xPos, 1.5f, 0.0f),
								new Vector3(xPos + kArrowWidth, 1 + kArrowHeight, 0.0f),
								new Vector3(xPos - kArrowWidth, 1 + kArrowHeight, 0.0f));
							}
							else
							{
								Handles.color = evnt.GetColor();
								Handles.DrawSolidDisc(new Vector3(xPos, evnt.GetBounds().yMax + kPinWidth, 0.0f), -Vector3.forward, kPinWidth);

								Handles.DrawAAConvexPolygon(new Vector3(xPos, 0, 0.0f),
								new Vector3(xPos + kArrowWidth, 0 + kArrowHeight, 0.0f),
								new Vector3(xPos - kArrowWidth, 0 + kArrowHeight, 0.0f));
							}
						}

						Handles.color = evnt.GetColor();
						Handles.DrawPolyLine(new Vector3(xPos, kArrowHeight), new Vector3(xPos, evnt.GetBounds().yMax + 0.5f));

						if (selected)
						{
							Handles.color = Color.white;
							Handles.DrawPolyLine(new Vector3(xPos - 1, kArrowHeight + 2), new Vector3(xPos - 1, evnt.GetBounds().yMax + 0.5f));
							Handles.DrawPolyLine(new Vector3(xPos - 2, kArrowHeight + 2), new Vector3(xPos - 2, evnt.GetBounds().yMax + 0.5f));

							Handles.DrawPolyLine(new Vector3(xPos + 1, kArrowHeight + 2), new Vector3(xPos + 1, evnt.GetBounds().yMin + EventEditorGUI.kDurationBoxY + 0.5f));
							Handles.DrawPolyLine(new Vector3(xPos + 2, kArrowHeight + 2), new Vector3(xPos + 2, evnt.GetBounds().yMin + EventEditorGUI.kDurationBoxY + 0.5f));

							Handles.DrawPolyLine(new Vector3(xPos + 1, evnt.GetBounds().yMax - EventEditorGUI.kShadowSize - 0.5f), new Vector3(xPos + 1, evnt.GetBounds().yMax + 0.5f));
							Handles.DrawPolyLine(new Vector3(xPos + 2, evnt.GetBounds().yMax - EventEditorGUI.kShadowSize - 0.5f), new Vector3(xPos + 2, evnt.GetBounds().yMax + 0.5f));
						}

						Handles.EndGUI();
					}
				}

				private static string GetCategory(Type eventType)
				{
					if (_eventCategory == null)
					{
						_eventCategory = new Dictionary<Type, string>();

						Type[] types = SystemUtils.GetAllSubTypes(typeof(Event));
						
						foreach (Type type in types)
						{
							EventCategoryAttribute eventAttribute = SystemUtils.GetAttribute<EventCategoryAttribute>(type);
							string category = string.Empty;

							if (eventAttribute != null && !string.IsNullOrEmpty(eventAttribute.Category))
							{
								category = eventAttribute.Category;
							}
							else
							{
								category = string.Empty;
							}

							_eventCategory.Add(type, category);
						}
					}

					string eventFullName;

					if (!_eventCategory.TryGetValue(eventType, out eventFullName))
					{
						throw new Exception("Type is not a kind of TimelineSystem.Event");
					}

					return eventFullName;
				}

				private void AddNewEventMenuCallback(object obj)
				{
					Type eventType = (Type)obj;
					CreateAndAddNewObject(eventType);
				}

				private static string GetEventMenuName(Type type)
				{
					string menuName = type.Name;

					if (menuName.StartsWith("Event"))
					{
						menuName = menuName.Substring("Event".Length, menuName.Length - "Event".Length);
					}
					menuName = StringUtils.FromCamelCase(menuName);

					return menuName;
				}
				#endregion
			}
		}
	}
}