using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Utils.Editor;

	namespace StateMachineSystem
	{
		namespace Editor
		{
			[StateCustomEditorGUI(typeof(StateMachineEntryState))]
			public class StateMachineEntryStateGUI : StateEditorGUI
			{
				public override void CalcRenderRect(StateMachineEditorStyle style)
				{
					GUIContent stateId = new GUIContent(Asset.GetEditorLabel());
					Vector2 labelDimensions = style._stateLabelStyle.CalcSize(stateId);
					
					float areaWidth = labelDimensions.x + style._shadowSize + (kMaxBorderSize * 2.0f);
					
					_rect.position = GetPosition();

					_rect.width = areaWidth;
					_rect.height = areaWidth * 0.75f;

					_rect.x -= areaWidth * 0.5f;
					_rect.y -= areaWidth * 0.5f;

					_rect.x = Mathf.Round(_rect.position.x);
					_rect.y = Mathf.Round(_rect.position.y);
				}

				public override void Render(Rect renderedRect, StateMachineEditorStyle style, Color borderColor, float borderSize)
				{
					GUIStyle labelStyle = style._stateLabelStyle;
					
					Color stateColor = Asset.GetEditorColor();

					//Update text colors to contrast state color
					{
						Color.RGBToHSV(stateColor, out _, out _, out float v);
						Color textColor = v > 0.66f ? Color.black : Color.white;
						labelStyle.normal.textColor = textColor;

						labelStyle.alignment = TextAnchor.MiddleCenter;
					}

					GUIContent stateId = new GUIContent(Asset.GetEditorLabel());
					borderSize = Mathf.Min(borderSize, kMaxBorderSize);
					float borderOffset = kMaxBorderSize - borderSize;

					Rect mainBoxRect = new Rect(renderedRect.x + kMaxBorderSize, renderedRect.y + kMaxBorderSize, renderedRect.width - style._shadowSize - (kMaxBorderSize * 2.0f), renderedRect.height - style._shadowSize - (kMaxBorderSize * 2.0f));

					//Draw shadow
					Rect shadowRect = new Rect(mainBoxRect.x + borderOffset + style._shadowSize, mainBoxRect.y + borderOffset + style._shadowSize, mainBoxRect.width + borderSize * 2f, mainBoxRect.height + borderSize * 2f);
					EditorUtils.DrawColoredRoundedBox(shadowRect, style._shadowColor, mainBoxRect.height);

					//Draw border
					Rect borderRect = new Rect(renderedRect.x + borderOffset, renderedRect.y + borderOffset, mainBoxRect.width + borderSize * 2f, mainBoxRect.height + borderSize * 2f);
					EditorUtils.DrawColoredRoundedBox(borderRect, borderColor, mainBoxRect.height);

					//Draw main box
					EditorUtils.DrawColoredRoundedBox(mainBoxRect, stateColor, mainBoxRect.height);

					//Draw label
					GUI.Label(mainBoxRect, stateId, labelStyle);
				}
			}
		}
	}
}