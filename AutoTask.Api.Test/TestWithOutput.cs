using AutoTask.Api.Config;
using AutoTask.Api.Test.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test;

/// <summary>Base class for integration tests that captures xUnit output and initialises AutoTask clients.</summary>
public class TestWithOutput
{
	/// <summary>Initializes a new instance of <see cref="TestWithOutput"/> and sets up clients from configuration.</summary>
	protected TestWithOutput(ITestOutputHelper iTestOutputHelper)
	{
		Logger = new XunitLogger(iTestOutputHelper);
		var nowUtc = DateTimeOffset.UtcNow;
		StartEpoch = nowUtc.AddDays(-30).ToUnixTimeSeconds();
		EndEpoch = nowUtc.ToUnixTimeSeconds();
		var configuration = LoadConfiguration("appsettings.json");
		var autoTaskCredentials = configuration.AutoTaskCredentials
			?? throw new ConfigurationException("Missing configuration.");

		ClientOptions? clientOptions = null;
		if (autoTaskCredentials.ServerId.HasValue)
		{
			clientOptions = new()
			{
				ServerId = autoTaskCredentials.ServerId.Value
			};
		}

		Client = new Client(
			autoTaskCredentials.Username,
			autoTaskCredentials.Password,
			autoTaskCredentials.IntegrationCode,
			Logger,
			clientOptions
			);
		AutoTaskClient = new AutoTaskClient(new AutoTaskConfiguration
		{
			Username = autoTaskCredentials.Username,
			Password = autoTaskCredentials.Password,
			IntegrationCode = autoTaskCredentials.IntegrationCode,
		});
		Stopwatch = Stopwatch.StartNew();
	}

	/// <summary>Loads test configuration from the specified JSON file and user secrets.</summary>
	protected static Configuration LoadConfiguration(string jsonFilePath)
	{
		var location = typeof(TestWithOutput).GetTypeInfo().Assembly.Location;
		var path1 = Path.GetDirectoryName(location) ?? throw new InvalidOperationException("path is null");
		var dirPath = Path.GetFullPath(Path.Combine(path1, "..", "..", ".."));

		Configuration configuration;
		var configurationRoot = new ConfigurationBuilder()
			.SetBasePath(dirPath)
			.AddUserSecrets<TestWithOutput>()
			.AddJsonFile(jsonFilePath, true, false)
			.Build();

		var services = new ServiceCollection();
		services.AddOptions();
		services.Configure<Configuration>(configurationRoot);
		using (var sp = services.BuildServiceProvider())
		{
			var options = sp.GetService<IOptions<Configuration>>();
			configuration = options!.Value;
		}

		return configuration;
	}

	/// <summary>Gets the logger used to write test output.</summary>
	protected ILogger Logger { get; }

	private Stopwatch Stopwatch { get; }

	/// <summary>Gets the Unix timestamp representing 30 days before the test run started.</summary>
	protected long StartEpoch { get; }

	/// <summary>Gets the Unix timestamp representing the moment the test run started.</summary>
	protected long EndEpoch { get; }

	/// <summary>Gets the <see cref="AutoTask.Api.Client"/> used by tests.</summary>
	protected Client Client { get; }

	/// <summary>Gets the <see cref="AutoTask.Api.AutoTaskClient"/> used by tests.</summary>
	protected AutoTaskClient AutoTaskClient { get; }

	/// <summary>Asserts that the elapsed test time is within the specified number of seconds.</summary>
	protected void AssertIsFast(int durationSeconds) => Assert.InRange(Stopwatch.ElapsedMilliseconds, 0, durationSeconds * 1000);
}