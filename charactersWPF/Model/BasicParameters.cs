using System;
using System.Windows;

namespace Characters.Model
{
	internal class BasicParameters
	{
		public double MaxWidth { get; set; }
		public double MaxHeight { get; set; }
		public double Radius { get; private set; }
		public int MaxNumberCharacterTypes { get; set; }
		public int MaxNumberCharacters { get; set; }
		public int PersonsCount { get; set; }
		public double Gplus { get; set; }
		public double Gminus { get; set; }
		public double Elasticity { get; set; }
		public double Viscosity { get; set; }
		public int TimeQuant { get; set; }
		public double Dimention => 100.0;
		public int DeathInterval => 1000;

		public int BurnDethThres { get; set; } = 10;

		public BasicParameters(
		    Size size, int radius, int maxNumberCharacterTypes, int maxNumberCharacters, int personsCount,
		    double gPlus, double gMinus, double elasticity, double viscosity, int timeQuant)
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
			Gplus = gPlus;
			Gminus = gMinus;
			Elasticity = elasticity;
			Viscosity = viscosity;
			TimeQuant = timeQuant;
		}
	}
}