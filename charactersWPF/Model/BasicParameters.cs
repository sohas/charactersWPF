using System.ComponentModel;
using System.Windows;

namespace Characters.Model
{
	internal class BasicParameters : INotifyPropertyChanged
	{
		private double g;
		private double gDelta;
		private const double timeQuantMilliseconsBase = 20;

		/// <summary>
		/// размер панели ширина
		/// </summary>
		public double MaxWidth
		{
			get; set;
		}

		/// <summary>
		/// размер панели высота
		/// </summary>
		public double MaxHeight
		{
			get; set;
		}

		/// <summary>
		/// радиус существа
		/// </summary>
		public double Radius
		{
			get; private set;
		}

		/// <summary>
		/// максимальное число разных видов характеров
		/// </summary>
		public int MaxNumberCharacterTypes
		{
			get; set;
		}

		/// <summary>
		/// максимальное число характеров в существе
		/// </summary>
		public int MaxNumberCharacters
		{
			get; set;
		}

		/// <summary>
		/// задаваемое число существ
		/// </summary>
		public int PersonsCount
		{
			get; set;
		}

		/// <summary>
		/// параметр взаимодействия со стеной 
		/// </summary>
		public double Elasticity
		{
			get; set;
		}

		/// <summary>
		/// параметр затухания скорости
		/// </summary>
		public double Viscosity
		{
			get; set;
		}

		/// <summary>
		/// средний коэффициент взаимодействия
		/// </summary>
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

		/// <summary>
		/// отклонение ("+" увеличивает притяжение и уменьшает отталкивание, "-" -- наоборот)
		/// </summary>
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

		/// <summary>
		/// стартовое время отведённое на итерацию
		/// </summary>
		public double TimeQuantMseconds { get; set; } = 20;

		/// <summary>
		/// коэффициент для согласования размерности
		/// </summary>
		public double Dimention => 100.0;

		/// <summary>
		/// порог для рождения и смерти
		/// </summary>
		public int BirthDethThreshold => 1;

		/// <summary>
		/// время умирания
		/// </summary>
		public int DeathIntervalMseconds => 300;

		/// <summary>
		/// время "полураспада"
		/// </summary>
		public double LifeTimeSeconds => 100;

		/// <summary>
		/// параметр коррекции сил притяжения/отталкивания в зависимости от числа существ
		/// получено эмпирически, как средний модуль базовой силы в завис от N
		/// </summary>
		public double ForceCorrection => 1.0 + (MaxNumberCharacters - 1) * 0.1;

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

		/// <summary>
		/// возврат времени итерации к базовому
		/// </summary>
		public void ResetTimeQuant() =>
			TimeQuantMseconds = timeQuantMilliseconsBase;

		private void NotifyPropertyChanged(string v) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
	}
}
