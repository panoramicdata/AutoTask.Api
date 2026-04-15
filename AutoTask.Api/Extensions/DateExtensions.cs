using System;
using System.Linq;

namespace AutoTask.Api.Extensions;

/// <summary>Extension methods for date and time formatting.</summary>
public static class DateExtensions
{
	private const string AutoTaskDateFormat = "yyyy-MM-ddTHH:mm:ss.fff";

	// Windows uses "Eastern Standard Time", Linux uses "EST"
	private static readonly TimeZoneInfo TimeZoneInfoEst =
		TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id == "Eastern Standard Time") ?
		TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") :
		TimeZoneInfo.FindSystemTimeZoneById("EST");

	/// <summary>Converts the <see cref="DateTimeOffset"/> to an AutoTask-formatted EST string.</summary>
	public static string ToEstString(this DateTimeOffset dateTimeOffset)
		=> dateTimeOffset.ToOffset(TimeZoneInfoEst.GetUtcOffset(dateTimeOffset)).ToString(AutoTaskDateFormat);
}
