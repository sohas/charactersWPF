using Characters.Model;

using System;
using System.IO;
using System.Resources;

namespace charactersWPF.Model
{
	internal static class PersonExtensions
	{
		internal static double GetBasicForceFrom(this double character1, double character2, BasicParameters parameters)
		{
			var diff = Math.Abs(character2 - character1);
			diff = 1 - 2 * Math.Abs(diff - 0.5); //0 -- одинаковые характеры, 1 -- максимально разные
			var repulsiveComponent = -diff * diff * parameters.Gminus;
			var attractiveComponent = (1 - diff) * (1 - diff) * parameters.Gplus;///

			return attractiveComponent + repulsiveComponent;
		}
	}

	public static class ResourceExtensions
	{
		public static MemoryStream GetMemoryStream(this ResourceManager resourceManager, String name)
		{
			object resource = resourceManager.GetObject(name);

			if (resource is byte[])
			{
				return new MemoryStream((byte[])resource);
			}
			else
			{
				throw new System.InvalidCastException("The specified resource is not a binary resource.");
			}
		}
	}
}