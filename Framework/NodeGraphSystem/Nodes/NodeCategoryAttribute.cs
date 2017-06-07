using System;

namespace Framework
{

	namespace NodeGraphSystem
	{
		public class NodeCategoryAttribute : Attribute
		{
#if UNITY_EDITOR
			public readonly string Category;
#endif

			public NodeCategoryAttribute(string category)
			{
#if UNITY_EDITOR
				Category = category;
#endif
			}
		}
	}
}