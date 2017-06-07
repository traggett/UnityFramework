using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputColorNode : OutputNode<NodeInputField<Color>, Color>
		{
		}
	}
}