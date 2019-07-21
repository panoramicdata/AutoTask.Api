using System;

namespace LogicMonitor.Integrator.Alerts.Exceptions
{
	public class FilterFieldNotPresentException : Exception
	{
		public FilterFieldNotPresentException()
		{
		}

		public FilterFieldNotPresentException(string message) : base(message)
		{
		}

		public FilterFieldNotPresentException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}