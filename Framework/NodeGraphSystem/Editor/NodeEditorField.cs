using UnityEngine;

using System;
using System.Reflection;

namespace Framework
{
	namespace NodeGraphSystem
	{
		namespace Editor
		{
			public sealed class NodeEditorField
			{
				public NodeEditorGUI _nodeEditorGUI;
				public Vector2 _position;
				public FieldInfo _fieldInfo;
				public Type _type;
				public string _name;
			}
		}
	}
}