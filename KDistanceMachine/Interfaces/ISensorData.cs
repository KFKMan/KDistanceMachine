public interface ISensorData<T>
{
	public Guid DataKey { get; set; }
	public DateTime LastRead { get; set; }
	public T Data { get; set; }

	public void SetData(T data)
	{
		DataKey = Guid.NewGuid();
		LastRead = DateTime.Now;
		Data = data;
	}
}
