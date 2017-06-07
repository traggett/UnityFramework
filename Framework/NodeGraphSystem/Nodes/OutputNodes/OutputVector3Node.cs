using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputVector3Node : OutputNode<NodeInputField<Vector3>, Vector3>
		{
		}
	}
}