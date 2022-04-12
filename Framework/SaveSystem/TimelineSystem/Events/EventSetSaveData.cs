using System;

using UnityEngine;

namespace Framework
{
	using TimelineSystem;

	namespace SaveSystem
	{
		namespace TimelineSystem
		{
			[Serializable]
			[EventCategory("SaveData")]
			public class EventSetSaveData : Event
			{
				public SaveDataValueRef<object> _saveDataProperty;
				public object _value;

				#region Event
				public override void Trigger()
				{
					_saveDataProperty.SetSaveDataValue(_value);
				}

#if UNITY_EDITOR
				public override Color GetEditorColor()
				{
					return new Color(0.9f, 0.83f, 0.15f);
				}

				public override string GetEditorDescription()
				{
					return "Set <b>" + _saveDataProperty + "</b> to <b>" + _value + "</b>";
				}
#endif
				#endregion
			}
		}
	}
}