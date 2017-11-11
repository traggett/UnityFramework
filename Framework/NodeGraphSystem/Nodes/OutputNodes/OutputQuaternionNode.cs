using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[Serializable]
		public class OutputQuaternionNode : OutputNode<NodeInputField<Quaternion>, Quaternion>
		{
		}
	}
}