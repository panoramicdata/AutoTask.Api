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

namespace AutoTask.Api.Test
{
	public class TestWithOutput
	{
		protected TestWithOutput(ITestOutputHelper iTestOutputHelper)
		{
			Logger = new XunitLogger(iTestOutputHelper);
			var nowUtc = DateTimeOffset.UtcNow;
			StartEpoch = nowUtc.AddDays(-30).ToUnixTimeSeconds();
			EndEpoch = nowUtc.ToUnixTimeSeconds();
			var configuration = LoadConfiguration("appsettings.json");
			var autoTaskCredentials = configuration.AutoTaskCredentials
				?? throw new System.Configuration.ConfigurationErrorsException("Missing configuration.");

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

		protected static Configuration LoadConfiguration(string jsonFilePath)
		{
			var location = typeof(TestWithOutput).GetTypeInfo().Assembly.Location;
			var path1 = Path.GetDirectoryName(location);
			if (path1 is null)
			{
				throw new InvalidOperationException("path is null");
			}
			var dirPath = Path.Combine(path1, "..\\..\\..");

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
				configuration = options.Value;
			}

			return configuration;
		}

		protected ILogger Logger { get; }

		private Stopwatch Stopwatch { get; }

		protected long StartEpoch { get; }

		protected long EndEpoch { get; }

		protected Client Client { get; }

		protected AutoTaskClient AutoTaskClient { get; }

		protected void AssertIsFast(int durationSeconds) => Assert.InRange(Stopwatch.ElapsedMilliseconds, 0, durationSeconds * 1000);
	}
}