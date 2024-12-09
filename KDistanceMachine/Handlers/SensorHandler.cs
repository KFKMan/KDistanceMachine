using KDistanceMachine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KDistanceMachine.Handlers
{
	public class SensorHandler<T> : ISensorHandler where T : struct
	{
		public ISensor<T> Sensor;

		public bool IsRunning { get; private set; } = false;

		public SensorHandler(ISensor<T> sensor)
		{
			Sensor = sensor;
		}

		public virtual void OnHandle(T data)
		{

		}

		public virtual async Task Handle(object? sensordata,CancellationToken token = default)
		{
			if (IsRunning)
			{
				return;
			}
			IsRunning = true;
			while (!token.IsCancellationRequested)
			{
				var data = await Sensor.TryGetValue(sensordata);
				if(data != null)
				{
					OnHandle(data.Value);
				}
			}
			IsRunning = false;
		}
	}
}
