using System.ComponentModel;
using System.Windows;

namespace Characters.Model
{
	internal class BasicParameters : INotifyPropertyChanged
	{
		private double g;
		private double gDelta;
		private const double timeQuantMilliseconsBase = 20;

		public double MaxWidth
		{
			get; set;
		}
		public double MaxHeight
		{
			get; set;
		}
		public double Radius
		{
			get; private set;
		}
		public int MaxNumberCharacterTypes
		{
			get; set;
		}
		public int MaxNumberCharacters
		{
			get; set;
		}
		public int PersonsCount
		{
			get; set;
		}
		public double Elasticity
		{
			get; set;
		}
		public double Viscosity
		{
			get; set;
		}
		public double G
		{
			get => g;
			set
			{
				if (value != g)
				{
					g = value;
					NotifyPropertyChanged("G");

					var newGdeltaValue = value + gDelta <= 0 ? 0.1 - value : value - gDelta <= 0 ? value - 0.1 : gDelta;
					Gdelta = newGdeltaValue;
				}
			}
		}

		public double Gdelta
		{
			get => gDelta;
			set
			{
				if (value != gDelta)
				{
					var newValue = G + value <= 0 ? 0.1 - G : G - value <= 0 ? G - 0.1 : value;
					gDelta = newValue;
					NotifyPropertyChanged("Gdelta");
				}
			}
		}

		public double TimeQuantMseconds { get; set; } = 20;
		public double Dimention => 100.0;
		public int BurnDethThreshold => 1;
		public int DeathIntervalMseconds => 300;
		public double LifeTimeSeconds => 100;
		public double ForceCorrection => 1.0 + (MaxNumberCharacters - 1) * 0.1;//получено эмпирически, как средний модуль базовой силы в завис от N

		public BasicParameters(
		    Size size, int radius, int maxNumberCharacterTypes, int maxNumberCharacters, int personsCount,
		    double g, double gDelta, double elasticity, double viscosity)
		{
			if (maxNumberCharacterTypes < 2 || maxNumberCharacters < 1)
			{
				throw new Exception("wrong parameters");
			}

			MaxWidth = size.Width;
			MaxHeight = size.Height;
			Radius = radius;
			MaxNumberCharacterTypes = maxNumberCharacterTypes;
			MaxNumberCharacters = maxNumberCharacters;
			PersonsCount = personsCount;
			this.g = g;
			this.gDelta = gDelta;
			Elasticity = elasticity;
			Viscosity = viscosity;
			TimeQuantMseconds = timeQuantMilliseconsBase;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public void ResetTimeQuant() =>
			TimeQuantMseconds = timeQuantMilliseconsBase;

		private void NotifyPropertyChanged(string v) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
	}
}
