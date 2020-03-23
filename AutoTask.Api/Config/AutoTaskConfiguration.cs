namespace AutoTask.Api.Config
{
	public class AutoTaskConfiguration
	{
		public string Username { get; set; } = null!;
		public string Password { get; set; } = null!;
		public string IntegrationCode { get; set; } = null!;

		internal void Validate()
		{
			if (string.IsNullOrWhiteSpace(Username))
			{
				throw new ConfigurationException($"{nameof(Username)} must be set.");
			}
			if (string.IsNullOrWhiteSpace(Password))
			{
				throw new ConfigurationException($"{nameof(Password)} must be set.");
			}
			if (string.IsNullOrWhiteSpace(IntegrationCode))
			{
				throw new ConfigurationException($"{nameof(IntegrationCode)} must be set.");
			}
		}
	}
}
