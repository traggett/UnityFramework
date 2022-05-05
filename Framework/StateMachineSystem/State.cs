using UnityEngine;

using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using Framework.Utils;
using UnityEditor;

namespace Framework
{
	namespace StateMachineSystem
	{
		[Serializable]
		public abstract class State : ScriptableObject
		{
			#region Public Data
			[HideInInspector]	
			public int _stateId = -1;

			//Editor properties
			[HideInInspector]
			public Vector2 _editorPosition = Vector2.zero;
			[HideInInspector]
			public bool _editorAutoDescription = true;
			[HideInInspector]
			public string _editorDescription;
			[HideInInspector]
			public bool _editorAutoColor = true;
			[HideInInspector]
			public Color _editorColor = Color.gray;
#if DEBUG
			[NonSerialized]
			public StateMachine _debugParentStateMachine;
#endif
			#endregion

			#region Public Interface
			public abstract IEnumerator PerformState(StateMachineComponent stateMachine);

#if UNITY_EDITOR
			public virtual string GetEditorLabel()
			{
				return "State" + _stateId.ToString("00") + " (" + GetType().Name + ")";
			}

			public virtual string GetEditorDescription() 
			{
				return GetType().Name;
			}

			public virtual Color GetEditorColor()
			{
				return new Color(61f / 255f, 154f / 255f, 92f / 255f);
			}

			public virtual StateMachineEditorLink[] GetEditorStateLinks()
			{
				//Could use serialsed properties??
				/*
				SerializedObject serialisedObject = new SerializedObject(this);
				SerializedProperty property = serialisedObject.GetIterator();
				List<SerializedProperty> stateRefProps = new List<SerializedProperty>();
				do
				{
					if (property.type == typeof(StateRef).Name)
					{
						stateRefProps.Add(property);
					}
				}
				while (property.Next(true));*/


				//Loop over member infos in state finding attrbute
				FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
				List<StateMachineEditorLink> links = new List<StateMachineEditorLink>();

				for (int i = 0; i < fields.Length; i++)
				{
					StateLinkAttribute attribute = SystemUtils.GetAttribute<StateLinkAttribute>(fields[i]);

					if (attribute != null)
					{
						if (fields[i].FieldType.IsArray)
						{
							Array array = fields[i].GetValue(this) as Array;

							for (int j = 0; j < array.Length; j++)
							{
								links.Add(new StateMachineEditorLink(attribute._editorName, this, fields[i], j));
							}
						}
						else
						{
							links.Add(new StateMachineEditorLink(attribute._editorName, this, fields[i]));
						}
					}
				}

				return links.ToArray();
			}
#endif
			#endregion
		}
	}
}