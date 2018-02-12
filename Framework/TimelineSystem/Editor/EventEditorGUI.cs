using System;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using Utils.Editor;
	using Utils;
	using Serialization;

	namespace TimelineSystem
	{
		namespace Editor
		{
			public class EventEditorGUI : SerializedObjectEditorGUI<Event>
			{
				#region Private Data
				private static Dictionary<Type, Type> _editorGUIConstructorMap = null;

				public static readonly float kShadowSize = 4.0f;
				public static readonly Color kShadowColor = new Color(0.0f, 0.0f, 0.0f, 0.35f);
				public static readonly Color kBorderColor = new Color(1, 1, 1, 0.66f);
				public static readonly Color kBorderColorHighlighted = new Color(1, 1, 1, 1);
				public readonly static float kDurationBoxHeight = 18.0f;
				public readonly static float kDurationBoxY = 8.0f;

				protected Rect _rect;
				#endregion

				#region Public Interface
				public float GetTime()
				{
					return GetEditableObject().GetTime();
				}

				public float GetDuration()
				{
					return GetEditableObject().GetDuration();
				}

				public bool EndsTimeline()
				{
					return GetEditableObject().GetEditorShouldBeLastEventInTimeline();
				}

				public void SetTime(float time)
				{
					GetEditableObject().SetTime(time);
					MarkAsDirty(true);
				}

				public void CalcBounds(Vector2 pos, float size, GUIStyle style)
				{
					Vector2 labelDimensions = GetLabelSize(style);

					float areaWidth = labelDimensions.x + kShadowSize + 2.0f;
					float areaHeight = labelDimensions.y + kShadowSize + kDurationBoxY + 2.0f;

					_rect.x = pos.x;
					_rect.y = pos.y;

					_rect.width = Mathf.Max(areaWidth, size);
					_rect.height = areaHeight;
				}

				public void SetBounds(Rect rect)
				{
					_rect = rect;
				}

				public void RenderOnTimeline(GUIStyle style, float size, bool isSelected)
				{
					Color origBackgroundColor = GUI.backgroundColor;
					GUI.backgroundColor = Color.clear;
					GUI.BeginGroup(_rect);
					{
						Vector2 labelSize = GetLabelSize(style);
						Rect labelRect = new Rect(1.0f, kDurationBoxY, labelSize.x, labelSize.y);

						//Draw shadow
						EditorUtils.DrawColoredHalfRoundedBox(new Rect(labelRect.x + kShadowSize, labelRect.y + kShadowSize, labelRect.width, labelRect.height), kShadowColor);

						//Draw area to show time line duration
						EditorUtils.DrawColoredHalfRoundedBox(new Rect(0.0f, 0.0f, size, kDurationBoxHeight), new Color(1.0f, 1.0f, 1.0f, isSelected ? 0.6f : 0.3f));
						
						//Draw outline
						EditorUtils.DrawColoredHalfRoundedBox(new Rect(labelRect.x - (isSelected ? 2.0f : 1.0f), labelRect.y - (isSelected ? 2.0f : 1.0f), labelRect.width + (isSelected ? 4.0f : 2.0f), labelRect.height + (isSelected ? 4.0f : 2.0f)), isSelected ? kBorderColorHighlighted : kBorderColor);

						//Draw colored box for label
						GUI.backgroundColor = GetColor();
						GUI.BeginGroup(labelRect, EditorUtils.ColoredHalfRoundedBoxStyle);
						{
							float h, s, v;
							Color.RGBToHSV(GetColor(), out h, out s, out v);
							style.normal.textColor = v > 0.66f ? Color.black : Color.white;

							//Draw Label
							DrawLabel(style);
						}
						GUI.EndGroup();
					}
					GUI.EndGroup();

					GUI.backgroundColor = origBackgroundColor;
				}

				public Color GetColor()
				{
					return GetEditableObject().GetEditorColor();
				}

				public static EventEditorGUI CreateEventEditorGUI(TimelineEditor editor, Event evnt)
				{
					if (_editorGUIConstructorMap == null)
					{
						_editorGUIConstructorMap = new Dictionary<Type, Type>();
						Type[] types = SystemUtils.GetAllSubTypes(typeof(EventEditorGUI));

						foreach (Type type in types)
						{
							EventCustomEditorGUIAttribute eventAttribute = SystemUtils.GetAttribute<EventCustomEditorGUIAttribute>(type);
							if (eventAttribute != null)
							{
								_editorGUIConstructorMap.Add(eventAttribute.EventType, type);
							}
						}
					}

					//Check for custom editor class
					Type editorGUIType;
					if (!_editorGUIConstructorMap.TryGetValue(evnt.GetType(), out editorGUIType))
					{
						//Use generic editor gui class
						editorGUIType = typeof(EventEditorGUI);
					}

					EventEditorGUI editorGUI = (EventEditorGUI)EventEditorGUI.CreateInstance(editorGUIType);
					editorGUI.Init(editor, evnt);

					return editorGUI;
				}
				#endregion

				#region Virtual Interface
				public virtual void DrawLabel(GUIStyle style)
				{
					GUI.Label(new Rect(0, 0, _rect.width, _rect.height), GetEditableObject().GetEditorDescription(), style);
				}

				public virtual Vector2 GetLabelSize(GUIStyle style)
				{
					string labelText = GetEditableObject().GetEditorDescription();
					GUIContent labelContent = new GUIContent(labelText);
					Vector2 size = style.CalcSize(labelContent) ;
					return size;
				}
				#endregion

				#region ICustomEditable
				public override bool RenderObjectProperties(GUIContent label)
				{
					bool dataChanged = RenderEventTime();
					
					SerializationEditorGUILayout.ObjectField(GetEditableObject(), "Event Properties", ref dataChanged);

					return dataChanged;
				}
				#endregion

				#region SerializedObjectEditorGUI
				protected override void OnSetObject()
				{

				}

				public override void SetPosition(Vector2 position)
				{
					_rect.position = position;
					MarkAsDirty(true);
				}

				public override Vector2 GetPosition()
				{
					return _rect.position;
				}

				public override Rect GetBounds()
				{
					return _rect;
				}

				public TimelineEditor GetTimelineEditor()
				{
					return (TimelineEditor)GetEditor();
				}
				#endregion

				protected bool RenderEventTime()
				{
					bool dataChanged = false;
					float time = 0.0f;
					GUIContent label = new GUIContent("Time");

					switch (GetTimelineEditor().GetTimeFormat())
					{
						case TimelineScrollArea.eTimeFormat.Default:
							{
								time = Event.RenderTimeField(label, GetEditableObject()._time, out dataChanged);
							}
							break;
						case TimelineScrollArea.eTimeFormat.DaysHoursMins:
							{
								time = Event.RenderDaysHoursMinsField(label, GetEditableObject()._time, out dataChanged);
							}
							break;
					}

					if (dataChanged)
					{
						GetEditableObject()._time = time;
						GetTimelineEditor().SetEventTime(this, time);
						return true;
					}

					return false;
				}

				#region IComparable
				public override int CompareTo(object obj)
				{
					EventEditorGUI evnt = obj as EventEditorGUI;

					if (evnt == null)
						return 1;

					if (evnt == this)
						return 0;

					return this.GetEditableObject().CompareTo(evnt.GetEditableObject());
				}
				#endregion
			}
		}
	}
}