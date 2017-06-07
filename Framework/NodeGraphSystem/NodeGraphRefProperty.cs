using UnityEngine;
using System;

namespace Framework
{
	using Serialization;

	namespace NodeGraphSystem
	{
		[Serializable]
		public struct NodeGraphRefProperty
		{
			#region Public Data
			public TextAsset _file;
			#endregion

			public NodeGraphRefProperty(TextAsset file=null)
			{
				_file = file;
			}

			public NodeGraph LoadNodeGraph()
			{
				if (_file != null)
				{
					return SerializeConverter.FromTextAsset<NodeGraph>(_file);
				}

				return null;
			}

			public bool IsValid()
			{
				return _file != null;
			}
		}
	}
}