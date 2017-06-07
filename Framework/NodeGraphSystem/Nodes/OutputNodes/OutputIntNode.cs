using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputIntNode : OutputNode<NodeInputField<int>, int>
		{
		}
	}
}