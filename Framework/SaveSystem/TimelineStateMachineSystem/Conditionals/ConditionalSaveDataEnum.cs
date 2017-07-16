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
		[ConditionalCategory("SaveData")]
		public class ConditionalSaveDataEnum : IConditional, ISerializationCallbackReceiver, ICustomEditorInspector
		{
			public SaveDataValueRef<Enum> _saveData;
			public int _value = -1;

#if UNITY_EDITOR
			private Enum _enumValue;
#endif

			#region IConditional
			public void OnStartConditionChecking(StateMachine stateMachine)
			{
			}

			public bool IsConditionMet(StateMachine stateMachine)
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

			public void OnEndConditionChecking(StateMachine stateMachine)
			{
			}
#if UNITY_EDITOR
			public string GetEditorDescription()
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

			public bool AllowInverseVariant()
			{
				return true;
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