using charactersWPF.Model;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Timer = System.Timers.Timer;

namespace Characters.Model
{
	internal class Person : INotifyPropertyChanged
	{
		public static BasicParameters? Parameters;

		#region fields
		private double x_LeftOnCanvas;
		private double y_TopOnCanvas;
		private double newX;
		private double newY;
		private double velocityX;
		private double velocityY;
		private double forceX;
		private double forceY;
		private double curPosImpact;
		private double curNegImpact;
		private double curWallImpact;
		private double lastPosImpact;
		private double lastNegImpact;
		private double lastWallImpact;

		private readonly DateTime birthTime;
		private PersonState state;
		private readonly Color meanColor;
		private Ellipse mainCircle;
		private Canvas mainCircleCanvas;
		private readonly double basicRotateAngle;

		private readonly List<double> characters;
		private readonly List<Color> charactersColors;
		private readonly Point chromeVector;
		private readonly int chromeStep;
		private readonly List<Person> persons;
		private readonly Canvas personsCanvas;

		private Timer killTimer = new Timer();
		#endregion

		#region public properties
		public double X_LeftOnCanvas
		{
			get
			{
				return x_LeftOnCanvas;
			}
			set
			{
				if (x_LeftOnCanvas != value)
				{
					x_LeftOnCanvas = value;
					NotifyPropertyChanged("X_LeftOnCanvas");
				}
			}
		}

		public double Y_TopOnCanvas
		{
			get
			{
				return y_TopOnCanvas;
			}
			set
			{
				if (y_TopOnCanvas != value)
				{
					y_TopOnCanvas = value;
					NotifyPropertyChanged("Y_TopOnCanvas");
				}
			}
		}

		public int ChromeStep => chromeStep;
		#endregion

		public event EventHandler Strike;
		public event PropertyChangedEventHandler? PropertyChanged;

		public Person(BasicParameters parameters, List<Person> persons, Canvas personsCanvas, object locker)
		{
			Parameters ??= parameters;
			this.birthTime = DateTime.Now;
			this.state = PersonState.NewBorn;

			this.persons = persons;
			lock (locker)
			{
				persons.Add(this);
			}

			var rnd = new Random();
			SetStartKinematic(rnd);

			this.characters = MakeCharecters(rnd);
			this.charactersColors = characters.Select(x => GetColor(x)).ToList();
			this.chromeVector = GetChromeVector(characters);
			var chromeAngle = GetChromeAngle(chromeVector);
			this.chromeStep = GetChromeStep(chromeAngle);
			this.meanColor = GetMeanColor(chromeAngle, GetChromeRadius(chromeVector));

			basicRotateAngle = rnd.NextDouble() * 5;
			BuildMainCanvasAndAllCircles();
			this.personsCanvas = personsCanvas;
			this.personsCanvas.Children.Add(mainCircleCanvas);

			killTimer.Interval = Parameters.DeathInterval;
			killTimer.Elapsed += Dying;

			OnStrike();
		}

		#region pubic methods
		public static void Iteration(List<Person> persons, ConcurrentBag<Person> deads, ConcurrentBag<Point> newBorns, object locker)
		{
			lock (locker)
			{
				foreach (var first in persons)
				{
					if (first.state == PersonState.Dead)
					{
						continue;
					}

					first.forceX = 0;
					first.forceY = 0;

					foreach (var second in persons)
					{
						if (first == second || second.state == PersonState.Dead)
						{
							continue;
						}

						first.SetForceAndImpactFromSecondPerson(second);
					}

					first.AddWallsReactionAndImpact();
					first.SetNewVelocity();
					first.AddViscosity();
					first.SetNewLocation();
					first.ProcessImpacts(deads, newBorns);
				}
			}

			lock (locker)
			{
				foreach (var person in persons)
				{
					person.X_LeftOnCanvas = person.newX;
					person.Y_TopOnCanvas = person.newY;
				}
			}
		}

		public void SetLocation(Point location)
		{
			X_LeftOnCanvas = newX = location.X;
			Y_TopOnCanvas = newY = location.Y;
		}

		public void Kill()
		{
			state = PersonState.Dead;

			mainCircle.Dispatcher.Invoke(() =>
			{
				var finalColor = Colors.White;

				ColorAnimation ca = new ColorAnimation(finalColor,
					new Duration(TimeSpan.FromMilliseconds(Parameters.DeathInterval)));

				foreach (Ellipse circle in mainCircleCanvas.Children)
				{
					circle.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ca);

					if (circle == mainCircle)
					{
						circle.Stroke.BeginAnimation(SolidColorBrush.ColorProperty, ca);
					}
				}

				mainCircleCanvas.RenderTransform = null;
			});

			killTimer.Start();
		}
		#endregion

		#region setup
		private void SetStartKinematic(Random rnd)
		{
			this.velocityX = 0;
			this.velocityY = 0;
			this.X_LeftOnCanvas = rnd.NextDouble() * (Parameters.MaxWidth - Parameters.Radius * 2);
			this.Y_TopOnCanvas = rnd.NextDouble() * (Parameters.MaxHeight - Parameters.Radius * 2);
		}

		private static Point GetChromeVector(List<double> characters)
		{
			double xAccum = 0;
			double yAccum = 0;

			if (characters.Count == 0)
			{
				return new Point(0, 0);
			}

			foreach (var character in characters)
			{
				var angle = Math.PI * 2 * character;
				xAccum += Math.Cos(angle);
				yAccum += Math.Sin(angle);
			}

			return new Point(xAccum / characters.Count, yAccum / characters.Count);
		}

		private static double GetChromeRadius(Point chromeVector)
		{
			var xAccum = chromeVector.X;
			var yAccum = chromeVector.Y;

			return Math.Sqrt(xAccum * xAccum + yAccum * yAccum);
		}

		private static double GetChromeAngle(Point chromeVector)
		{
			var xAccum = chromeVector.X;
			var yAccum = chromeVector.Y;

			var r = GetChromeRadius(chromeVector);
			var ang =
				r == 0 ?
				0 :
				yAccum >= 0 ?
				Math.Acos(xAccum / r) :
				Math.PI * 2 - Math.Acos(xAccum / r);

			return ang / (Math.PI * 2);
		}

		private static int GetChromeStep(double chromeAngle)
		{
			var res = (int)(chromeAngle * Parameters.MaxNumberCharacterTypes);
			res = res % 2 == 0 ? res / 2 * 7 : (res - 1) / 2 * 7 + 4;
			res %= 24;

			return res;
		}

		private static List<double> MakeCharecters(Random rnd)
		{
			var charactersNumber = rnd.Next(1, Parameters.MaxNumberCharacters + 1);
			var res = new List<double>();

			for (var i = 0; i < charactersNumber; i++)
			{
				var characterIndex = rnd.Next(0, Parameters.MaxNumberCharacterTypes);
				var character = characterIndex / (double)Parameters.MaxNumberCharacterTypes;//от 0 до 1 исключая 1
				res.Add(character);
			}
			res.Sort();

			return res;
		}

		private static Color GetColor(double index0)
		{
			var index = index0 * 1.5;
			index = index == 1.5 ? 0 : index;

			double r = 0;
			double g = 0;
			double b = 0;

			if (index >= 0 && index < 0.5)
			{
				r = -index * 2 + 1;
				g = index * 2 + 0;
				b = 0;
			}
			else if (index >= 0.5 && index < 1)
			{
				r = 0;
				g = -index * 2 + 2;
				b = index * 2 - 1;
			}
			else if (index >= 1 && index < 1.5)
			{
				r = index * 2 - 2;
				g = 0;
				b = -index * 2 + 3;
			}

			var k = 1 / Math.Pow((r * r + g * g + b * b), 0.5);

			var R = (int)(k * r * 255);
			R = R < 0 ? 0 : R > 255 ? 255 : R;
			var G = (int)(k * g * 255);
			G = G < 0 ? 0 : G > 255 ? 255 : G;
			var B = (int)(k * b * 255);
			B = B < 0 ? 0 : B > 255 ? 255 : B;

			return Color.FromRgb((byte)R, (byte)G, (byte)B);
		}

		private static Color GetMeanColor(double chromeAngle, double chromeR)
		{
			var res = GetColor(chromeAngle);
			var baseK = 1 / 2.0;
			res.A = (byte)(res.A * chromeR * (1 - (1 - baseK) / Parameters.MaxNumberCharacters));
			return res;
		}

		private void BuildMainCanvasAndAllCircles()
		{
			mainCircleCanvas = new Canvas()
			{
				Width = Parameters.Radius * 2,
				Height = Parameters.Radius * 2,
				Background = Brushes.Transparent,
				DataContext = this,
			};

			Binding blc = new Binding("X_LeftOnCanvas");
			blc.Mode = BindingMode.TwoWay;
			mainCircleCanvas.SetBinding(Canvas.LeftProperty, blc);

			Binding btc = new Binding("Y_TopOnCanvas");
			btc.Mode = BindingMode.TwoWay;
			mainCircleCanvas.SetBinding(Canvas.TopProperty, btc);

			mainCircle = new Ellipse()
			{
				Width = Parameters.Radius * 2,
				Height = Parameters.Radius * 2,
				Fill = new SolidColorBrush(meanColor),
				Stroke = new SolidColorBrush(Brushes.White.Color),
				StrokeThickness = 0.1,
				DataContext = this,
			};

			mainCircle.SetValue(Canvas.LeftProperty, 0.0);
			mainCircle.SetValue(Canvas.TopProperty, 0.0);
			mainCircleCanvas.Children.Add(mainCircle);

			var sina0 = Math.Sin(Math.PI / Parameters.MaxNumberCharacters);
			var d = 2 * Parameters.Radius * sina0 / (1 + sina0);

			for (var i = 0; i < charactersColors.Count; i++)
			{
				var a = i * 2 * Math.PI / charactersColors.Count;

				var ell = new Ellipse()
				{
					Width = d,
					Height = d,
					Fill = new SolidColorBrush(charactersColors[i])
				};

				var dx = Parameters.Radius + (Parameters.Radius - d / 2) * Math.Sin(a) - d / 2;
				var dy = Parameters.Radius - (Parameters.Radius - d / 2) * Math.Cos(a) - d / 2;
				ell.SetValue(Canvas.LeftProperty, dx);
				ell.SetValue(Canvas.TopProperty, dy);
				mainCircleCanvas.Children.Add(ell);
			}

			mainCircleCanvas.RenderTransform = new RotateTransform(0, Parameters.Radius, Parameters.Radius);

			var rotateAnimation = new DoubleAnimation()
			{
				From = 0,
				By = 360,
				Duration = TimeSpan.FromMilliseconds(Parameters.TimeQuant * 360 / (basicRotateAngle == 0 ? 1 : basicRotateAngle)),
				RepeatBehavior = RepeatBehavior.Forever,
			};
			mainCircleCanvas.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
		}
		#endregion

		#region dynamics
		private void SetForceAndImpactFromSecondPerson(Person second)
		{
			double force = 0;
			curNegImpact = 0;
			curPosImpact = 0;

			var distanceX = second.X_LeftOnCanvas - this.X_LeftOnCanvas;
			var distanceY = second.Y_TopOnCanvas - this.Y_TopOnCanvas;
			var distanceSQ = distanceX * distanceX + distanceY * distanceY;
			var distance = Math.Sqrt(distanceSQ);

			foreach (var character in this.characters)
			{
				foreach (var secondCharacter in second.characters)
				{
					force += character.GetBasicForceFrom(secondCharacter, Parameters);
				}
			}

			force /= Parameters.ForceCorrection;

			if (distance < 2 * Parameters.Radius)
			{
				if (force < 0)
				{
					curNegImpact = force;
				}
				else
				{
					curPosImpact = force;
				}

				force = -Parameters.Elasticity;
			}
			else
			{
				force /= 0.1 + distanceSQ / (Parameters.Dimention * Parameters.Dimention);
			}

			if (distance == 0)
			{
				distance = distanceX = distanceY = 1;
			}

			forceX += (distanceX / distance) * force;
			forceY += (distanceY / distance) * force;
		}

		private void SetNewVelocity()
		{
			velocityX += forceX * Parameters.TimeQuant / Parameters.Dimention;
			velocityY += forceY * Parameters.TimeQuant / Parameters.Dimention;
		}

		private void AddViscosity()
		{
			var vsqr = (velocityX * velocityX + velocityY * velocityY);
			var visc = Parameters.Viscosity / (Parameters.Dimention * Parameters.Dimention * Parameters.Dimention);

			velocityX /= (1 + vsqr * visc);
			velocityY /= (1 + vsqr * visc);
		}

		private void SetNewLocation()
		{
			newX = this.X_LeftOnCanvas + (velocityX * Parameters.TimeQuant) / Parameters.Dimention;
			newY = this.Y_TopOnCanvas + (velocityY * Parameters.TimeQuant) / Parameters.Dimention;
		}

		private void AddWallsReactionAndImpact()
		{
			curWallImpact = 0;

			if (X_LeftOnCanvas < 0 && velocityX <= 0)
			{
				velocityX = -velocityX;
				X_LeftOnCanvas = 0;
				curWallImpact -= Parameters.Elasticity;
				OnStrike();
			}
			else if (X_LeftOnCanvas > Parameters.MaxWidth - Parameters.Radius * 2 && velocityX >= 0)
			{
				velocityX = -velocityX;
				X_LeftOnCanvas = Parameters.MaxWidth - Parameters.Radius * 2;
				curWallImpact -= Parameters.Elasticity;
				OnStrike();
			}

			if (Y_TopOnCanvas < 0 && velocityY <= 0)
			{
				velocityY = -velocityY;
				Y_TopOnCanvas = 0;
				curWallImpact -= Parameters.Elasticity;
				OnStrike();
			}
			else if (Y_TopOnCanvas > Parameters.MaxHeight - Parameters.Radius * 2 && velocityY >= 0)
			{
				velocityY = -velocityY;
				Y_TopOnCanvas = Parameters.MaxHeight - Parameters.Radius * 2;
				curWallImpact -= Parameters.Elasticity;
				OnStrike();
			}
		}

		private void ProcessImpacts(ConcurrentBag<Person> deads, ConcurrentBag<Point> newBorns)
		{
			lastPosImpact = curPosImpact == 0 ? 0 : lastPosImpact + curPosImpact;
			lastNegImpact = curNegImpact == 0 ? 0 : lastNegImpact + curNegImpact;
			lastWallImpact = curWallImpact == 0 ? 0 : lastWallImpact + curWallImpact;

			if (
				lastWallImpact < -Parameters.Elasticity * Parameters.BurnDethThreshold ||
				lastNegImpact < -(Parameters.G - Parameters.Gdelta) * Parameters.BurnDethThreshold)
			{
				deads.Add(this);
			}

			if (lastPosImpact > (Parameters.G + Parameters.Gdelta) * Parameters.BurnDethThreshold && this.state != PersonState.Dead)
			{
				lastPosImpact = 0;
				curPosImpact = 0;
				newBorns.Add(new Point(this.x_LeftOnCanvas, this.y_TopOnCanvas));
			}
		}

		private void Dying(object sender, ElapsedEventArgs e)
		{
			personsCanvas?.Dispatcher.Invoke(() => personsCanvas.Children.Remove(mainCircleCanvas));
			killTimer.Stop();
			killTimer.Dispose();
			OnStrike();
		}
		#endregion

		private void NotifyPropertyChanged(string v)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
		}

		private void OnStrike() 
		{
			Strike?.Invoke(this, EventArgs.Empty);
		}
	}
}