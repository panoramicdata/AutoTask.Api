using System.Reflection;
using System.Runtime.ExceptionServices;

namespace AutoTask.Api.Test.Unit;

/// <summary>Invokes non-public members, rethrowing the real exception rather than a reflection wrapper.</summary>
internal static class PrivateAccess
{
	private const BindingFlags AnyMember = BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

	internal static object? Invoke(Type type, object? target, string methodName, params object?[] arguments)
	{
		var method = type.GetMethod(methodName, AnyMember) ?? throw new MissingMethodException(type.Name, methodName);
		try
		{
			return method.Invoke(target, arguments);
		}
		catch (TargetInvocationException exception) when (exception.InnerException is not null)
		{
			ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
			throw;
		}
	}
}
