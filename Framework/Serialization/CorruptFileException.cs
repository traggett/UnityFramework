using System;

namespace Framework
{
	using Utils;

	namespace Serialization
	{
		public class CorruptFileException : Exception
		{
			public CorruptFileException(Exception exception) : base("Corrupt File", exception)
			{

			}
		}
	}
}