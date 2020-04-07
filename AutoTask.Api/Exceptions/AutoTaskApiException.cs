using System;
using System.Linq;
using System.Runtime.Serialization;

namespace AutoTask.Api.Exceptions
{
	[Serializable]
	internal class AutoTaskApiException : Exception
	{
		public ATWSResponse? Response { get; }

		public AutoTaskApiException(ATWSResponse queryResult) : base(string.Join(", ", queryResult.Errors.Select(e => e.Message)))
		{
			Response = queryResult;
		}

		public AutoTaskApiException(string message) : base(message)
		{
			Response = null;
		}

		public AutoTaskApiException(string message, Exception innerException) : base(message, innerException)
		{
			Response = null;
		}

		protected AutoTaskApiException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			Response = null;
		}

		public AutoTaskApiException() : base()
		{
		}
	}
}
