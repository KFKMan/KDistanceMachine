using KDistanceMachine.Interfaces;
using System.Linq.Expressions;

namespace KDistanceMachine.Handlers
{
	public class AppDataHandler<T> : SensorHandlerWithData<T> where T : struct
	{
		private AppData _instance;
		private Action<AppData, ISensorData<T>> _propertySetter;

		public AppDataHandler(ISensor<T> sensor, AppData instance, Expression<Func<AppData, ISensorData<T>>> propertyExpression) : base(sensor)
		{
			_instance = instance;
			_propertySetter = CreatePropertySetter(propertyExpression);
			OnData += OnNewData;
		}

		public void OnNewData(object? sender,EventArgs e)
		{
			UpdateProperty(Data);
		}

		private Action<AppData, ISensorData<T>> CreatePropertySetter(Expression<Func<AppData, ISensorData<T>>> propertyExpression)
		{
			var memberExpression = (MemberExpression)propertyExpression.Body;
			var parameter = Expression.Parameter(typeof(ISensorData<T>), "value");

			var assign = Expression.Lambda<Action<AppData, ISensorData<T>>>(
				Expression.Assign(memberExpression, Expression.Convert(parameter, memberExpression.Type)),
				propertyExpression.Parameters.Single(), parameter);

			return assign.Compile();

			/*
			var memberExpression = (MemberExpression)propertyExpression.Body;
			var parameter = Expression.Parameter(typeof(ISensorData<T>), "value");
			var assign = Expression.Lambda<Action<AppData, ISensorData<T>>>(
				Expression.Assign(memberExpression, parameter),
				propertyExpression.Parameters.Single(), parameter);
			return assign.Compile(); */
		}

		public void UpdateProperty(ISensorData<T> data)
		{
			_propertySetter(_instance, data);
		}
	}
}
