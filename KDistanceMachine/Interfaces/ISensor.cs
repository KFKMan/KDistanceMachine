namespace KDistanceMachine.Interfaces
{

	public interface ISensor<T> where T : struct
	{
		public Task<T?> TryGetValue(object? data);
	}
}
