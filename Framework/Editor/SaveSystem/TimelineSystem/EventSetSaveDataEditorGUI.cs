using UnityEngine;

namespace Framework
{
	using Framework.TimelineSystem.Editor;
	using Serialization;

	namespace SaveSystem
	{
		namespace TimelineSystem
		{
			namespace Editor
			{
				//The event should store a xmlnode child of unspecified type that matches the type of the property foudn in the SaveDataValueProperty
				//This can be edited and updated on the event.
				[EventCustomEditorGUI(typeof(EventSetSaveData))]
				public class EventSetSaveDataEditorGUI : EventEditorGUI
				{
					#region EventEditorGUI
					protected override void OnSetObject()
					{

					}

					public override bool RenderObjectProperties(GUIContent label)
					{
						EventSetSaveData evnt = GetEditableObject() as EventSetSaveData;

						bool dataChanged = false;

						//Want to render the save data block normally but
						bool saveDataChanged = false;
						evnt._saveDataProperty = SerializationEditorGUILayout.ObjectField(evnt._saveDataProperty, GUIContent.none, ref saveDataChanged);

						//If the save data block or property has changed, update our saved property value node.
						if (saveDataChanged)
						{
							evnt._value = evnt._saveDataProperty.CreateEditorValueInstance();
							dataChanged = true;
						}

						if (evnt._value != null)
						{
							evnt._value = SerializationEditorGUILayout.ObjectField(evnt._value, new GUIContent("Value"), ref dataChanged);
						}

						return dataChanged;
					}
					#endregion
				}
			}
		}
	}
}