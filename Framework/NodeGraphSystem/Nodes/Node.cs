using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Framework
{
	using Utils;
	
	namespace NodeGraphSystem
	{
		public abstract class Node
		{
			#region Public Data
			[HideInInspector]
			public int _nodeId = -1;
			[HideInInspector]
			public Vector2 _editorPosition = Vector2.zero;
			[HideInInspector]
			public string _editorDescription = string.Empty;
			#endregion

			public virtual void Init() { }

			public virtual void Update(float deltaTime) { }

#if UNITY_EDITOR
			public virtual Color GetEditorColor()
			{
				return Color.grey;
			}

			public FieldInfo[] GetEditorInputFields()
			{
				List<FieldInfo> xmlMembers = new List<FieldInfo>();

				FieldInfo[] fields = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
				foreach (FieldInfo field in fields)
				{
					if (SystemUtils.IsSubclassOfRawGeneric(typeof(NodeInputFieldBase<>), field.FieldType))
					{
						xmlMembers.Add(field);
					}
				}

				return xmlMembers.ToArray();
			}
#endif
		}
	}
}