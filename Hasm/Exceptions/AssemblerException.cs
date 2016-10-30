using System;

namespace hasm.Exceptions
{
	internal sealed class AssemblerException : Exception
	{
		public AssemblerException()
		{
		}

		public AssemblerException(string message) : base(message)
		{
		}

		public AssemblerException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}