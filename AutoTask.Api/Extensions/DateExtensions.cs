using System;

namespace AutoTask.Api.Extensions;

/// <summary>Extension methods for date and time formatting.</summary>
public static class DateExtensions
{
	private const string AutoTaskDateFormat = "yyyy-MM-ddTHH:mm:ss.fff";

	// Windows uses "Eastern Standard Time"; Linux uses IANA ids, and some images ship no "EST" file.
	private static readonly TimeZoneInfo TimeZoneInfoEst = FindEasternTimeZone();

	private static TimeZoneInfo FindEasternTimeZone()
	{
		foreach (var id in new[] { "Eastern Standard Time", "America/New_York", "EST" })
		{
			if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var timeZone))
			{
				return timeZone;
			}
		}

		throw new TimeZoneNotFoundException("No Eastern time zone is available on this machine.");
	}

	/// <summary>Converts the <see cref="DateTimeOffset"/> to an AutoTask-formatted EST string.</summary>
	public static string ToEstString(this DateTimeOffset dateTimeOffset)
		=> dateTimeOffset.ToOffset(TimeZoneInfoEst.GetUtcOffset(dateTimeOffset)).ToString(AutoTaskDateFormat);
}
