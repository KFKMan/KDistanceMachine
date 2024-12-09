namespace KDistanceMachine.Handlers
{
	public class SensorData<T> : ISensorData<T> where T : struct
	{
		public Guid DataKey { get; set; } = Guid.Empty;
		public DateTime LastRead { get; set; } = DateTime.MinValue;
		public T Data { get; set; } = default;
	}
}
