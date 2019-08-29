using System;
using System.Runtime.Serialization;

namespace AutoTask.Api.Exceptions
{
	[Serializable]
	internal class AutoTaskApiException : Exception
	{
		public AutoTaskApiException()
		{
		}

		public AutoTaskApiException(string message) : base(message)
		{
		}

		public AutoTaskApiException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AutoTaskApiException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}
