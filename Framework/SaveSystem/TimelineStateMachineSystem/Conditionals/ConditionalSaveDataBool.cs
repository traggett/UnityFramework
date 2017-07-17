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
		[ConditionalCategory("SaveData")]
		public class ConditionalSaveDataBool : IConditional, ICustomEditorInspector
		{		
			public SaveDataValueRef<bool> _saveData;
			public bool _value = false;

			#region IConditional
#if UNITY_EDITOR
			public string GetEditorDescription()
			{
				return "(<b>" + _saveData + "</b>) is (<b>" + _value + "</b>)";
			}

			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				_saveData  = SerializationEditorGUILayout.ObjectField(_saveData, GUIContent.none, ref dataChanged);
				
				EditorGUI.BeginChangeCheck();
				_value = EditorGUILayout.Toggle("Value", _value);
				dataChanged |= EditorGUI.EndChangeCheck();

				return dataChanged;
			}

			public bool AllowInverseVariant()
			{
				return true;
			}
#endif

			public void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
			}

			public bool IsConditionMet(StateMachineComponent stateMachine)
			{
				object boolNode = _saveData.GetSaveDataValue();

				if (boolNode != null && SystemUtils.IsTypeOf(typeof(bool), boolNode.GetType()))
				{
					return (bool)boolNode == _value;
				}

				return false;
			}

			public void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
			#endregion
		}
	}
}