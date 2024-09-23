namespace charactersWPF.Model
{
	class Statistics
	{
		private int lastItemsCount = 0;

		private readonly Queue<double> timeQuantQueue = new Queue<double>();
		private const int timeQuantQueueMaxLenth = 10;
		private const double timeQuantMin = 10.0;
		private const double timeQuantAccuracy = 5.0;
		private double meanTimeQuantSum = 0;//суммируется по всей очереди
		private double lastMeanTimeQuantSum = 0;

		private readonly Queue<double> temperatureQueue = new Queue<double>();
		private const int temperatureQueueMaxLength = 100;
		private const double temperatureAccuracy = 10.0;
		private double meanTemperatureSum = 0;//суммируется по всей очереди
		private double lastMeanTemperatureSum = 0;

		private readonly Queue<double> pressureQueue = new Queue<double>();
		private const int pressureQueueMaxLength = 50;
		private const double pressureAccuracy = 1.0;
		private double meanPressure = 0;
		private double lastMeanPressure = 0;

		/// <summary>
		/// число частиц
		/// </summary>
		public int ItemsCount { get; set; }

		/// <summary>
		/// не средняя энергия частиц, а суммарная
		/// </summary>
		public double TemperatureAccum { get; set; }

		/// <summary>
		/// давление как суммарное число ударов на единицу длины периметра
		/// </summary>
		public double PressureAccum { get; set; }

		/// <summary>
		/// периметр панели с элементами
		/// </summary>
		public double Perimeter { get; set; }

		/// <summary>
		/// период таймера итерации
		/// </summary>
		public double TimeQuant { get; set; }

		public event EventHandler<int>? ItemsCountChanged;
		public event EventHandler<double>? TemperatureChanged;
		public event EventHandler<double>? PressureChanged;
		public event EventHandler<double>? TimeQuantChanged;

		public Statistics(double basicTimeQuant) 
		{
			lastMeanTimeQuantSum = basicTimeQuant * timeQuantQueueMaxLenth;//чтобы срабатывало на уменьшение в начальных итерациях
		}

		/// <summary>
		/// чтобы привести необходимые параметры к стартовому состоянию
		/// </summary>
		public void StartGettingStatisticsInIteration()
		{
			TemperatureAccum = 0;
			PressureAccum = 0;
		}

		/// <summary>
		/// проверка на изменения параметров
		/// </summary>
		public void CheckStatistics()
		{
			CheckTimeQuant();
			CheckItemCount();
			CheckTemperature();
			CheckPressure();
		}

		private void CheckTimeQuant()
		{
			timeQuantQueue.Enqueue(TimeQuant);
			meanTimeQuantSum += TimeQuant;

			while (timeQuantQueue.Count > timeQuantQueueMaxLenth)
			{
				meanTimeQuantSum -= timeQuantQueue.Dequeue();
			}

			if (
				timeQuantQueue.Count == timeQuantQueueMaxLenth && 
				Math.Abs(lastMeanTimeQuantSum - meanTimeQuantSum) > timeQuantQueueMaxLenth * timeQuantAccuracy)
			{
				var newTimeQuant = meanTimeQuantSum / timeQuantQueueMaxLenth;
				
				newTimeQuant = 
					newTimeQuant < timeQuantMin ? 
					timeQuantMin : 
					newTimeQuant; 

				TimeQuantChanged?.Invoke(this, newTimeQuant);
				lastMeanTimeQuantSum = meanTimeQuantSum;
			}
		}

		private void CheckItemCount() 
		{
			if (lastItemsCount != ItemsCount)
			{
				ItemsCountChanged?.Invoke(this, ItemsCount);
				lastItemsCount = ItemsCount;
			}
		}

		private void CheckTemperature()
		{
			if (ItemsCount == 0) 
			{
				return;
			}

			var newTemperature = TemperatureAccum / ItemsCount;//получаем среднюю по всем частицам
			temperatureQueue.Enqueue(newTemperature);
			meanTemperatureSum += newTemperature;

			while (temperatureQueue.Count > temperatureQueueMaxLength)
			{
				meanTemperatureSum -= temperatureQueue.Dequeue();
			}

			if (
				temperatureQueue.Count == temperatureQueueMaxLength && 
				Math.Abs(lastMeanTemperatureSum - meanTemperatureSum) > temperatureQueueMaxLength * temperatureAccuracy)
			{
				TemperatureChanged?.Invoke(this, meanTemperatureSum / temperatureQueueMaxLength);
				lastMeanTemperatureSum = meanTemperatureSum;
			}
		}

		private void CheckPressure() 
		{
			if (Perimeter == 0) 
			{
				return;
			}

			var newPressure = PressureAccum / Perimeter;
			pressureQueue.Enqueue(newPressure);
			meanPressure += newPressure;

			while (pressureQueue.Count > pressureQueueMaxLength)
			{
				meanPressure -= pressureQueue.Dequeue();
			}

			if (
				pressureQueue.Count == pressureQueueMaxLength && 
				Math.Abs(lastMeanPressure - meanPressure) > pressureAccuracy)
			{
				PressureChanged?.Invoke(this, meanPressure);
				lastMeanPressure = meanPressure;
			}
		}
	}
}