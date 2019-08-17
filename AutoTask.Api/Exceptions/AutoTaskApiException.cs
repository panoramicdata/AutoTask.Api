using AutoTask.Api.Extensions;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace AutoTask.Api.Exceptions
{
	[Serializable]
	internal class AutoTaskApiException : Exception
	{
		public AutoTaskApiException(ATWSResponse atwsResponse)
			: base(atwsResponse.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and "))
		{
		}

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
