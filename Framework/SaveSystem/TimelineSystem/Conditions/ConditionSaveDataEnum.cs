using System;

using UnityEngine;

namespace Framework
{
	using StateMachineSystem;
	using Utils;

	namespace SaveSystem
	{
		namespace TimelineSystem
		{
			[Serializable]
			[ConditionCategory("SaveData")]
			public class ConditionSaveDataEnum : ToggableCondition, ISerializationCallbackReceiver
			{
				public SaveDataValueRef<Enum> _saveData;
				public int _value = -1;

#if UNITY_EDITOR
				[NonSerialized]
				public Enum _enumValue;
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
					return "If (" + _saveData + " is " + _enumValue + ")";
				}

				public override string GetTakenText()
				{
					return _saveData + " is " + _enumValue;
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
				public Type GetEnumType()
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
}