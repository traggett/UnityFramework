using UnityEngine;
using System;

namespace Framework
{
	namespace NodeGraphSystem
	{
		[Serializable]
		public class OutputColorNode : OutputNode<NodeInputField<Color>, Color>
		{
		}
	}
}