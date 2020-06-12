namespace AutoTask.Api
{
	public class ClientOptions
	{
		public int OpenTimeoutMs { get; set; } = 10000;

		public int SendTimeoutMs { get; set; } = 30000;

		public int? ServerId { get; set; }
	}
}