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
					GUIStyle labelStyle = style._stateLabelStyle;
					Vector2 labelDimensions = labelStyle.CalcSize(new GUIContent("W"));

					float areaWidth = (labelDimensions.y * 3f) + style._shadowSize + (kMaxBorderSize * 2.0f);
					
					_rect.position = GetPosition();

					_rect.width = areaWidth;
					_rect.height = areaWidth * 0.66f;

					_rect.x -= areaWidth * 0.5f;
					_rect.y -= areaWidth * 0.5f;

					_rect.x = Mathf.Round(_rect.position.x);
					_rect.y = Mathf.Round(_rect.position.y);
				}

				public override void Render(Rect renderedRect, StateMachineEditorStyle style, Color borderColor, float borderSize)
				{
					Color stateColor = Asset.GetEditorColor();

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
					Texture icon = EditorGUIUtility.IconContent("StepButton").image;
					Rect labelRect = new Rect(mainBoxRect.x + (mainBoxRect.width - icon.width)  * 0.5f, mainBoxRect.y + (mainBoxRect.height - icon.height) * 0.5f, icon.width, icon.height);
					GUI.DrawTexture(labelRect, EditorGUIUtility.IconContent("StepButton").image);
				}
			}
		}
	}
}