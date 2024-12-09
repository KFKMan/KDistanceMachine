using KDistanceMachine.Handlers;
using DistanceSensorData = ISensorData<UnitsNet.Length>;

public class AppData
{
	public AppData()
	{
		DistanceData = new SensorData<UnitsNet.Length>();
		BaseData = new();
	}

	public DistanceSensorData DistanceData { get; set; }

	public BaseData BaseData { get; set; }
}

public class BaseData
{
	public int HorizontalFOV { get; set; }
	public int VerticalFOV { get; set; }

	public int QualityWidth { get; set; }
	public int QualityHeight { get; set; }
}