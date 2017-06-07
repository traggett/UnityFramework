using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[NodeCategory("Output Nodes")]
		[Serializable]
		public class OutputFloatNode : OutputNode<NodeInputField<float>, float>
		{
		}
	}
}