using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputQuaternionNode : OutputNode<NodeInputField<Quaternion>, Quaternion>
		{
		}
	}
}