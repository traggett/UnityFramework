using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputVector2Node : OutputNode<NodeInputField<Vector2>, Vector2>
		{
		}
	}
}