using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitsNet;

namespace KDistanceMachine.Interfaces
{
	public interface ISensorHandler
	{
		Task Handle(object? sensordata, CancellationToken token = default);
	}
}
