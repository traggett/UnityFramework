using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Input Nodes")]
		[Serializable]
		public class ComponentInputNode : InputNode<Component>
		{
		}
	}
}