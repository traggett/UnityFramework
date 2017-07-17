using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
	using StateMachineSystem;
	using SaveSystem;
	using UnityEngine;
	using Serialization;

	namespace TimelineStateMachineSystem
	{
		[Serializable]
		[ConditionCategory("SaveData")]
		public class ConditionSaveDataEnum : ToggableCondition, ISerializationCallbackReceiver, ICustomEditorInspector
		{
			public SaveDataValueRef<Enum> _saveData;
			public int _value = -1;

#if UNITY_EDITOR
			private Enum _enumValue;
#endif

			#region Conditional
			public override void OnStartConditionChecking(StateMachineComponent stateMachine)
			{
			}

			public override bool IsConditionMet(StateMachineComponent stateMachine)
			{
				object enumNode = _saveData.GetSaveDataValue();

				if (enumNode != null && enumNode.GetType().IsEnum)
				{
					//Get child with id as int, compare to our value
					int enumIntValue = Convert.ToInt32(enumNode);
					return _value == enumIntValue;
				}

				return false;
			}

			public override void OnEndConditionChecking(StateMachineComponent stateMachine)
			{
			}
#if UNITY_EDITOR
			public override string GetDescription()
			{
				if (_enumValue != null)
				{
					return "(<b>" + _saveData + "</b>) is (<b>" + _enumValue + "</b>)";
				}
				else
				{
					return "(<b>" + _saveData + "</b>) is (<b><value></b>)";
				}
			}

			public override string GetTakenText()
			{
				return GetDescription();
			}
#endif
			#endregion

			#region ISerializationCallbackReceiver
			public void OnBeforeSerialize()
			{

			}

			public void OnAfterDeserialize()
			{
#if UNITY_EDITOR
				Type enumType = GetEnumType();

				if (enumType != null)
				{
					_enumValue = (Enum)Enum.ToObject(enumType, _value);
				}
#endif
			}
			#endregion

#if UNITY_EDITOR
			public bool RenderObjectProperties(GUIContent label)
			{
				bool dataChanged = false;

				//Save data type (only show enum properties)
				_saveData = SerializationEditorGUILayout.ObjectField(_saveData, GUIContent.none, ref dataChanged);

				//Possible values
				Type enumType = GetEnumType();

				if (enumType != null)
				{
					Enum enm = (Enum)Enum.ToObject(enumType, _value);
					_enumValue = EditorGUILayout.EnumPopup("Value", enm);
					_value = Convert.ToInt32(_enumValue);
				}
				else
				{
					_enumValue = null;
					_value = -1;
				}

				return dataChanged;
			}
#endif
			

#if UNITY_EDITOR
			private Type GetEnumType()
			{
				object enumNode = _saveData.CreateEditorValueInstance();

				if (enumNode != null && enumNode.GetType().IsEnum)
				{
					return enumNode.GetType();
				}

				return null;
			}
#endif
		}
	}
}