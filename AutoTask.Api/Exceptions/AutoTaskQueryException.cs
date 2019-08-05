using AutoTask.Api.Extensions;
using System;
using System.Linq;

namespace AutoTask.Api.Exceptions
{
	[Serializable]
	internal class AutoTaskApiException : Exception
	{
		private readonly ATWSResponse _atwsResponse;

		public AutoTaskApiException(ATWSResponse atwsResponse)
			=> _atwsResponse = atwsResponse;

		public AutoTaskApiException()
		{
		}

		public AutoTaskApiException(string message) : base(message)
		{
		}

		public AutoTaskApiException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AutoTaskApiException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}

		public override string Message => _atwsResponse.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and ");
	}
}
