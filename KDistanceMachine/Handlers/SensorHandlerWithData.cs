using KDistanceMachine.Interfaces;

namespace KDistanceMachine.Handlers
{
	public class SensorHandlerWithData<T> : SensorHandler<T>, ISensorHandlerWithData<T> where T : struct
	{
		public ISensorData<T> Data { get; set; } = new SensorData<T>();

#pragma warning disable CS8618
		public SensorHandlerWithData(ISensor<T> sensor) : base(sensor)
#pragma warning restore CS8618
		{

		}


		public event EventHandler OnData;

		public override void OnHandle(T data)
		{
			Data.SetData(data);
			OnData?.Invoke(this,new());
		}
	}
}
