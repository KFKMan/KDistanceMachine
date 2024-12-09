namespace KDistanceMachine.Interfaces
{
	public interface ISensorHandlerWithData<T> : ISensorHandler
	{
		ISensorData<T> Data { get; }
		public event EventHandler OnData;
	}
}
