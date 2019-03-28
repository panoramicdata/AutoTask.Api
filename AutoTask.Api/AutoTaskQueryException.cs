using AutoTask.Api.Extensions;
using System;
using System.Linq;

namespace AutoTask.Api
{
	[Serializable]
	internal class AutoTaskQueryException : Exception
	{
		private readonly ATWSResponse _atwsResponse;

		public AutoTaskQueryException(ATWSResponse atwsResponse)
		{
			_atwsResponse = atwsResponse;
		}

		public AutoTaskQueryException()
		{
		}

		public AutoTaskQueryException(string message) : base(message)
		{
		}

		public AutoTaskQueryException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AutoTaskQueryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}

		public override string Message => _atwsResponse.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and ");
	}
}
