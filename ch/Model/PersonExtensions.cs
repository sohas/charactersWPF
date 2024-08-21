using Characters.Model;

using System;

namespace charactersWPF.Model
{
	internal static class PersonExtensions
	{
		internal static double GetBasicForceFrom(this double character1, double character2, BasicParameters parameters)
		{
			//var attractiveComponent = character1 == character2 ? parameters.Gplus : 0;
			var diff = Math.Abs(character2 - character1);
			diff = 1 - 2 * Math.Abs(diff - 0.5); //0 -- одинаковые характеры, 1 -- максимально разные
			var repulsiveComponent = - diff * diff * parameters.Gminus;
			var attractiveComponent = (1 - diff) * (1 - diff) * parameters.Gplus;///

			return attractiveComponent + repulsiveComponent;
		}
	}
}