namespace AutoTask.Api.Config
{
	public class AutoTaskConfiguration
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string IssueProblemSignatureField { get; set; } = "2";
	}
}
