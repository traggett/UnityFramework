using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputVector4Node : OutputNode<NodeInputField<Vector4>, Vector4>
		{
		}
	}
}