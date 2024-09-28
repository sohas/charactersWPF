using Characters.Model;
using System.Windows.Controls;

namespace charactersWPF.Model
{
	internal static class PersonExtensions
	{
		/// <summary>
		/// считает силу взаимодействия между 2 характерами
		/// </summary>
		/// <param name="character1">характер 1</param>
		/// <param name="character2">характер 2</param>
		/// <param name="parameters">класс параметров</param>
		/// <returns></returns>
		internal static double GetBasicForceFrom(this double character1, double character2, BasicParameters parameters)
		{
			var diff = Math.Abs(character2 - character1);
			diff = 1 - 2 * Math.Abs(diff - 0.5); //0 -- одинаковые характеры, 1 -- максимально разные
			var repulsiveComponent = -diff * diff * (parameters.G - parameters.Gdelta);
			var attractiveComponent = (1 - diff) * (1 - diff) * (parameters.G + parameters.Gdelta);///

			return (attractiveComponent + repulsiveComponent) / Math.Sqrt(parameters.Dimention);
		}
	}

	//public static class ResourceExtensions
	//{
	//	public static MemoryStream GetMemoryStream(this ResourceManager resourceManager, String name)
	//	{
	//		object resource = resourceManager.GetObject(name);

	//		if (resource is byte[])
	//		{
	//			return new MemoryStream((byte[])resource);
	//		}
	//		else
	//		{
	//			throw new System.InvalidCastException("The specified resource is not a binary resource.");
	//		}
	//	}
	//}

	public static class UIElementExtensions
	{
		/// <summary>
		/// обновляет индиктор числовым значением
		/// </summary>
		/// <param name="indicator">индиктор</param>
		/// <param name="e">значение</param>
		public static void UpdateIndicator(this Label indicator, double e)
		{
			indicator?.Dispatcher?.Invoke(() => indicator.Content = $"{indicator.Tag}={e:F0}");
		}

		/// <summary>
		/// обновляет индикатор строковым значением
		/// </summary>
		/// <param name="indicator">индикатор</param>
		/// <param name="e">значение</param>
		public static void UpdateIndicator(this Label indicator, string e)
		{
			indicator?.Dispatcher?.Invoke(() => indicator.Content = $"{indicator.Tag}={e}");
		}
	}
}
