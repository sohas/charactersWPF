using System;
using System.ComponentModel;
using System.Windows;

namespace Characters.Model
{
	internal class BasicParameters : INotifyPropertyChanged
	{
		private double gDelta;

		public double MaxWidth { get; set; }
		public double MaxHeight { get; set; }
		public double Radius { get; private set; }
		public int MaxNumberCharacterTypes { get; set; }
		public int MaxNumberCharacters { get; set; }
		public int PersonsCount { get; set; }
		public double Elasticity { get; set; }
		public double Viscosity { get; set; }
		public double G { get; set; }

		public double Gdelta
		{
			get => gDelta;
			set
			{
				if (gDelta != value && gDelta <= G && gDelta >= -G)
				{
					gDelta = value;
					NotifyPropertyChanged("Gdelta");
				}
			}
		}

		public int TimeQuant => 20;
		public double Dimention => 100.0;
		public int BurnDethThreshold => 1;
		public int DeathInterval => 1000;
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
			G = g;
			this.gDelta = gDelta;
			Elasticity = elasticity;
			Viscosity = viscosity;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		private void NotifyPropertyChanged(string v)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
		}
	}
}