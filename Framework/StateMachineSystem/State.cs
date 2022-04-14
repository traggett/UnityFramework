using UnityEngine;

using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using Framework.Utils;

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
			public virtual StateMachineEditorLink[] GetStateLinks()
			{
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
								StateMachineEditorLink link = new StateMachineEditorLink
								{
									_state = this,
									_fieldInfo = fields[i],
									_arrayIndex = j,
									_description = attribute._editorName
								};

								links.Add(link);
							}
						}
						else
						{
							StateMachineEditorLink link = new StateMachineEditorLink
							{
								_state = this,
								_fieldInfo = fields[i],
								_arrayIndex = -1,
								_description = attribute._editorName
							};

							links.Add(link);
						}
					}
				}

				return links.ToArray();
			}

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
#endif
			#endregion
		}
	}
}