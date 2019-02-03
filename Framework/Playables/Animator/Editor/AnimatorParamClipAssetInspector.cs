using Framework.Utils;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace Framework
{
	namespace Playables
	{
		namespace Editor
		{
			//[CustomEditor(typeof(AnimatorParamClipAsset))]
			public class AnimatorParamClipAssetInspector : UnityEditor.Editor
			{
				protected ReorderableList _floatParams;

				public void OnEnable()
				{
					_floatParams = new ReorderableList(new AnimatorPlayableBehaviour.FloatParam[0], typeof(AnimatorPlayableBehaviour.FloatParam), false, true, true, true)
					{
						drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawHeader),
						drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawFloatParam),
						onAddCallback = new ReorderableList.AddCallbackDelegate(OnAddFloatParam),
						onRemoveCallback = new ReorderableList.RemoveCallbackDelegate(OnRemoveFloatParam),
						showDefaultBackground = true,
						index = 0,
						elementHeight = 20f
					};
				}

				public override void OnInspectorGUI()
				{
					AnimatorParamClipAsset animatorClip = target as AnimatorParamClipAsset;
					if (animatorClip == null)
						return;

					GUILayout.Label(animatorClip.name, EditorStyles.boldLabel);
					
					GUILayout.Label("Float Params", EditorStyles.boldLabel);
					GUILayout.Space(3f);
					_floatParams.list = new List<AnimatorPlayableBehaviour.FloatParam>(animatorClip._data._floatParams);
					_floatParams.DoLayoutList();

					animatorClip._data._floatParams = new AnimatorPlayableBehaviour.FloatParam[_floatParams.list.Count];
					_floatParams.list.CopyTo(animatorClip._data._floatParams, 0);
				}

				private void OnAddFloatParam(ReorderableList list)
				{
					AnimatorParamClipAsset animatorClip = target as AnimatorParamClipAsset;
					if (animatorClip == null)
						return;

					_floatParams.list.Add(new AnimatorPlayableBehaviour.FloatParam());

					//Add curve for this float?
					animatorClip._timelineClip.curves.SetCurve("_data._floatParams[0]", typeof(AnimatorPlayableBehaviour.FloatParam), "_value", new AnimationCurve());
				}

				private void OnRemoveFloatParam(ReorderableList list)
				{
					AnimatorParamClipAsset animatorClip = target as AnimatorParamClipAsset;
					if (animatorClip == null)
						return;

					_floatParams.list.RemoveAt(_floatParams.index);

					//Remove curve for this float?
				}

				private void CreateSubTrack(object data)
				{
					
				}

				protected virtual void OnDrawHeader(Rect rect)
				{
					float columnWidth = rect.width /= 2f;
					GUI.Label(rect, "Name", EditorStyles.label);
					rect.x += columnWidth;
					GUI.Label(rect, "Value", EditorStyles.label);
				}

				protected virtual void OnDrawFloatParam(Rect rect, int index, bool selected, bool focused)
				{
					float columnWidth = rect.width / 2f;
					AnimatorPlayableBehaviour.FloatParam param = (AnimatorPlayableBehaviour.FloatParam)_floatParams.list[index];
					rect.width = columnWidth;
					param._id =  EditorGUI.TextField(rect, param._id);
					rect.x += columnWidth;
					param._value = EditorGUI.FloatField(rect, param._value, EditorStyles.numberField);

				}
			}
		}
	}
}