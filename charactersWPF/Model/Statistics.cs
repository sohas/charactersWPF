using System;
using System.Collections.Generic;

namespace charactersWPF.Model
{
	class Statistics
	{
		private Queue<double> pressureQueue = new Queue<double>();
		private Queue<double> temperatureQueue = new Queue<double>();
		private readonly int times;
		private double meanPressure = 0;
		private double meanTemperature = 0;

		private int lastN = 0;
		private double lastMeanTemperature = 0;
		private double lastMeanPressure = 0;

		public int N { get; set; }
		public double Temperature { get; set; }
		public double Pressure { get; set; }
		public double Perimeter { get; set; }

		public event EventHandler<int> NChanged;
		public event EventHandler<double> TemperatureChanged;
		public event EventHandler<double> PressureChanged;

		public Statistics(int times)
		{
			this.times = times;
		}

		public void StartGettingStatistics()
		{
			Temperature = 0;
			Pressure = 0;
			N = 0;
		}

		public void CheckStatistics()
		{
			if (lastN != N)
			{
				NChanged?.Invoke(this, N);
			}

			if (N != 0)
			{
				var newTemperature = Temperature / N;
				temperatureQueue.Enqueue(newTemperature);
				meanTemperature += newTemperature;

				while (temperatureQueue.Count > times)
				{
					meanTemperature -= temperatureQueue.Dequeue();
				}

				if (Math.Abs(lastMeanTemperature - meanTemperature) > times * 10)
				{
					TemperatureChanged?.Invoke(this, meanTemperature / times);
					lastMeanTemperature = meanTemperature;
				}
			}

			if (Perimeter != 0)
			{
				var newPressure = Pressure / Perimeter;
				pressureQueue.Enqueue(newPressure);
				meanPressure += newPressure;

				while (pressureQueue.Count > times)
				{
					meanPressure -= pressureQueue.Dequeue();
				}

				if (Math.Abs(lastMeanPressure - meanPressure) > 1)
				{
					PressureChanged?.Invoke(this, meanPressure);
					lastMeanPressure = meanPressure;
				}
			}
		}
	}
}