using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace Framework
{
	using Utils;
	using StateMachineSystem;
	using SaveSystem;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionCategory("SaveData")]
		public class ConditionSaveDataBool : Condition, ICustomEditorInspector
		{		
			public SaveDataValueRef<bool> _saveData;
			public bool _value = false;

			#region Conditional
#if UNITY_EDITOR
			public override string GetDescription()
			{
				return "(<b>" + _saveData + "</b>) is (<b>" + _value + "</b>)";
			}

			public override string GetTakenText()
			{
				return GetDescription();
			}
#endif

			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
			}

			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				object boolNode = _saveData.GetSaveDataValue();

				if (boolNode != null && SystemUtils.IsTypeOf(typeof(bool), boolNode.GetType()))
				{
					return (bool)boolNode == _value;
				}

				return false;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
			#endregion

			#region Conditional
#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				_saveData = SerializationEditorGUILayout.ObjectField(_saveData, GUIContent.none, ref dataChanged);

				EditorGUI.BeginChangeCheck();
				_value = EditorGUILayout.Toggle("Value", _value);
				dataChanged |= EditorGUI.EndChangeCheck();

				return dataChanged;
			}
#endif
			#endregion
		}
	}
}