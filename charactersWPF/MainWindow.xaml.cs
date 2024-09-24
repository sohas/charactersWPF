using Characters.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using Timer = System.Timers.Timer;
using charactersWPF.Model;
using System.Collections.Concurrent;
using System.Timers;
using System.Diagnostics;

namespace charactersWPF
{
	public partial class MainWindow : Window
	{
		#region fields
		private readonly Grid mainGrid = new();
		private readonly DockPanel controlDockPanel = new();
		private readonly StackPanel indicatorsCanvas = new();
		private readonly Canvas personsCanvas = new();
		private readonly SolidColorBrush personsCanvasBackBrush = Brushes.Black;
		private readonly Button closeButton = new Button();
		private readonly Button fullButton = new Button();
		private readonly Button minimizeButton = new Button();
		private readonly Button startButton = new Button();
		private readonly Button newButton = new Button();
		private readonly Button manualButton = new Button();
		private Label indicatorN;
		private Label indicatorTemperature;
		private Label indicatorPressure;
		private Label indicatorTimeQuant;
		private Thickness uiMargin;
		private double basicH;

		private readonly List<Person> persons = new();
		private readonly object locker = new object();
		private readonly ConcurrentBag<Person> deads = new();
		private readonly ConcurrentBag<Point> newBorns = new();
		private Timer iterationTimer = new Timer();
		private bool isStarted = false;
		private readonly BasicParameters parameters;
		private System.Windows.WindowState currentState = System.Windows.WindowState.Normal;
		private Brush backColor;
		private readonly Dictionary<int, Player> soundPlayers = new();
		private bool canAutoChangeGdelta = true;
		private int lastPersonsCount = 0;
		private Statistics statistics;
		private bool canIterate = true;
		#endregion

		public MainWindow()
		{
			var size = new Size(600, 600);
			var radius = 20;
			var maxNumberCharacterTypes = 24;
			var maxNumberCharacters = 24;
			var personsCount = 20;
			var g = 5;
			var gDelta = 0.0;
			var elasticity = 3;
			var viscosity = 6;

			parameters = new BasicParameters(
			    size, radius, maxNumberCharacterTypes, maxNumberCharacters, personsCount,
			    g, gDelta, elasticity, viscosity);

			MakeDesign();
			SetupSoundPlayers();
		}

		private void SetupIterationTimer()
		{
			iterationTimer = new Timer() { Interval = parameters.TimeQuantMseconds };
			iterationTimer.Elapsed += OnIteration;
		}

		private void SetupSoundPlayers()
		{
			soundPlayers[0] = new Player("Notes/01MC.mp3");
			soundPlayers[1] = new Player("Notes/02MCs.mp3");
			soundPlayers[2] = new Player("Notes/03MD.mp3");
			soundPlayers[3] = new Player("Notes/04MDs.mp3");
			soundPlayers[4] = new Player("Notes/05ME.mp3");
			soundPlayers[5] = new Player("Notes/06MF.mp3");
			soundPlayers[6] = new Player("Notes/07MFs.mp3");
			soundPlayers[7] = new Player("Notes/08MG.mp3");
			soundPlayers[8] = new Player("Notes/09MGs.mp3");
			soundPlayers[9] = new Player("Notes/10MA.mp3");
			soundPlayers[10] = new Player("Notes/11MAs.mp3");
			soundPlayers[11] = new Player("Notes/12MH.mp3");
			soundPlayers[12] = new Player("Notes/13MC.mp3");
			soundPlayers[13] = new Player("Notes/14MCs.mp3");
			soundPlayers[14] = new Player("Notes/15MD.mp3");
			soundPlayers[15] = new Player("Notes/16MDs.mp3");
			soundPlayers[16] = new Player("Notes/17ME.mp3");
			soundPlayers[17] = new Player("Notes/18MF.mp3");
			soundPlayers[18] = new Player("Notes/19MFs.mp3");
			soundPlayers[19] = new Player("Notes/20MG.mp3");
			soundPlayers[20] = new Player("Notes/21MGs.mp3");
			soundPlayers[21] = new Player("Notes/22MA.mp3");
			soundPlayers[22] = new Player("Notes/23MAs.mp3");
			soundPlayers[23] = new Player("Notes/24MH.mp3");
		}

		private void SetupStatistics()
		{
			statistics = new Statistics(parameters.TimeQuantMseconds);
			statistics.ItemsCountChanged += (o, e) => indicatorN?.UpdateIndicator(e);
			statistics.TemperatureChanged += (o, e) => indicatorTemperature?.UpdateIndicator(e);
			statistics.PressureChanged += (o, e) => indicatorPressure?.UpdateIndicator(e);
			statistics.TimeQuantChanged += (o, e) =>
			{
				iterationTimer.Interval = e;
				parameters.TimeQuantMseconds = e;
				indicatorTimeQuant.UpdateIndicator(e);
			};
		}

		#region design
		private void MakeDesign()
		{
			var basicStyle = WindowStyle.SingleBorderWindow;
			var noneStyle = WindowStyle.None;
			backColor = SystemColors.WindowFrameBrush;

			this.BorderThickness = new Thickness(0);
			this.Content = this.mainGrid;
			this.Icon = null;
			this.Title = "Characters";
			this.Margin = new();

			this.mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Auto) });
			this.mainGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(100, GridUnitType.Star) });
			this.mainGrid.Margin = new Thickness(0);
			this.mainGrid.Height = this.Height;
			this.mainGrid.Width = this.Width;
			this.mainGrid.ShowGridLines = false;
			this.mainGrid.Background = backColor;

			this.controlDockPanel.Background = backColor;
			this.mainGrid.Children.Add(this.controlDockPanel);
			Grid.SetColumn(controlDockPanel, 0);
			Grid.SetRow(controlDockPanel, 0);

			this.closeButton.Background = backColor;
			this.closeButton.Foreground = Brushes.White;
			this.closeButton.Visibility = Visibility.Collapsed;
			this.closeButton.Margin = new(0);
			this.closeButton.BorderThickness = new Thickness(0);
			this.closeButton.HorizontalAlignment = HorizontalAlignment.Right;
			this.closeButton.Content = " X ";
			this.closeButton.Width = 25;
			this.closeButton.Click += (o, e) => Close();
			this.controlDockPanel.Children.Add(closeButton);
			DockPanel.SetDock(closeButton, Dock.Right);

			this.fullButton.Background = backColor;
			this.fullButton.Foreground = Brushes.White;
			this.fullButton.Margin = new(0);
			this.fullButton.BorderThickness = new Thickness(0);
			this.fullButton.HorizontalAlignment = HorizontalAlignment.Right;
			this.fullButton.Content = " O ";
			this.fullButton.Width = 25;
			this.fullButton.Click += (o, e) =>
			{
				if (this.WindowStyle == noneStyle)
				{
					this.WindowState = currentState;
					this.WindowStyle = basicStyle;
					this.closeButton.Visibility = Visibility.Collapsed;
					this.minimizeButton.Visibility = Visibility.Collapsed;
				}
				else
				{
					currentState = this.WindowState;
					this.WindowState = System.Windows.WindowState.Maximized;
					this.WindowStyle = noneStyle;
					this.closeButton.Visibility = Visibility.Visible;
					this.minimizeButton.Visibility = Visibility.Visible;
				}
			};
			DockPanel.SetDock(fullButton, Dock.Right);
			this.controlDockPanel.Children.Add(fullButton);

			this.minimizeButton.Background = backColor;
			this.minimizeButton.Foreground = Brushes.White;
			this.minimizeButton.Visibility = Visibility.Collapsed;
			this.minimizeButton.Margin = new(0);
			this.minimizeButton.BorderThickness = new Thickness(0);
			this.minimizeButton.HorizontalAlignment = HorizontalAlignment.Right;
			this.minimizeButton.Content = "__";
			this.minimizeButton.Width = 25;
			this.minimizeButton.Click +=
				(o, e) => this.WindowState = System.Windows.WindowState.Minimized;
			DockPanel.SetDock(minimizeButton, Dock.Right);
			this.controlDockPanel.Children.Add(minimizeButton);

			uiMargin = new Thickness(0, 0, 10, 0);

			this.manualButton.Background = backColor;
			this.manualButton.Foreground = Brushes.White;
			this.manualButton.Margin = uiMargin;
			this.manualButton.BorderThickness = new Thickness(0);
			this.manualButton.Content = "?";
			this.manualButton.Width = 20;
			this.controlDockPanel.Children.Add(manualButton);
			this.manualButton.Click += (o, e) => 
			{
				System.Windows.MessageBox.Show("при нажатии кнопки \"New\" начнётся жинь круглых существ. круглое существо обладает характерами — маленькими цветными шариками внутри существа. два существа притягиваются, если их характеры похожи, или отталкиваются, если характеры различаются. близость характеров определяется близостью их цветов на цветовом круге: например, красный и оранжевый характеры близки, а зелёный и фиолетовый — разные.\r\nпритягиваясь своими характерами, существа движутся друг к другу, отталкиваясь, разлетаются. но даже притягивающиеся существа, сблизившись слишком сильно, разлетаются. все существа отскакивают от стенок.\r\nможно задавать стартовое число существ (\"N\"), число типовв характеров (\"Tp\") и максимальное число характеров существа (\"Ch\") даже когда запущена игра.\r\nсущества могут умирать от старости, из-за долгого взаимодействия со стеной или с отвратительным для него существом. если приятные друг другу существа долго близки, они могут родить новых существ. если существ больше, чем их стартовое число, вероятность смерти возрастает, а вероятность рождения растёт. и наоборот.\r\nсущества могут умирать, рождаться, собираться в группы, разлетаться, и это может длиться вечно.\r\nслева вверху на экране показаны N — текущее число существ, T — температура газа существ, P — давление на стенки, Q — время итерации в миллисекундах.\r\nзвук имеет значение.");
			};

			basicH = startButton.Height;

			this.startButton.Background = backColor;
			this.startButton.Foreground = Brushes.White;
			this.startButton.Margin = uiMargin;
			this.startButton.BorderThickness = new Thickness(0);
			this.startButton.Height = basicH;
			this.startButton.Content = "Start/Pause";
			this.startButton.Click += StartContinue;
			this.controlDockPanel.Children.Add(newButton);

			this.newButton.Background = backColor;
			this.newButton.Foreground = Brushes.White;
			this.newButton.Margin = uiMargin;
			this.newButton.BorderThickness = new Thickness(0);
			this.newButton.Height = basicH;
			this.newButton.Content = "New";
			this.newButton.Click += New;
			this.controlDockPanel.Children.Add(startButton);


			var chTypesNumberLabel = MakeAndAddLabel("Tp", new());
			chTypesNumberLabel.Margin = new Thickness();

			var chTypesNumberInput = MakeAndAddInput(parameters.MaxNumberCharacterTypes, uiMargin);
			chTypesNumberInput.ValueChanged +=
				(o, e) => parameters.MaxNumberCharacterTypes = (int)chTypesNumberInput.Value;
			chTypesNumberInput.Minimum = 1;
			chTypesNumberInput.Maximum = 24;

			var chNumberLabel = MakeAndAddLabel("Ch", new());

			var chNumberInput = MakeAndAddInput(parameters.MaxNumberCharacters, uiMargin);
			chNumberInput.ValueChanged +=
				(o, e) => parameters.MaxNumberCharacters = (int)chNumberInput.Value;
			chNumberInput.Minimum = 1;
			chNumberInput.Maximum = 24;

			var personsNumberLabel = MakeAndAddLabel("N", new());

			var personsCountInput = MakeAndAddInput(parameters.PersonsCount, uiMargin);
			personsCountInput.Minimum = 2;
			personsCountInput.Maximum = 100;
			personsCountInput.ValueChanged += (o, e) =>
			{
				parameters.PersonsCount = (int)personsCountInput.Value;
				canAutoChangeGdelta = true;
			};

			var gLabel = MakeAndAddLabel("G", new());

			var gInput = MakeAndAddInput(parameters.G, uiMargin, "G");
			gInput.Minimum = 1;

			var gDeltaLabel = MakeAndAddLabel("dG", new());

			var gDeltaInput = MakeAndAddInput(parameters.Gdelta, uiMargin, "Gdelta");
			gDeltaInput.ValueChanged += (o, e) =>
			{
				gDeltaInput.Text = $"{gDeltaInput.Value:F2}";
				lastPersonsCount = persons.Count;
				canAutoChangeGdelta = false;
			};

			var elasticityLabel = MakeAndAddLabel("E", new());

			var elasticityInput = MakeAndAddInput(parameters.Elasticity, uiMargin);
			elasticityInput.ValueChanged +=
				(o, e) => parameters.Elasticity = (double)elasticityInput.Value;
			elasticityInput.Minimum = 0;

			var viscosityLabel = MakeAndAddLabel("V", new());

			var viscosityInput = MakeAndAddInput(parameters.Viscosity, uiMargin);
			viscosityInput.ValueChanged +=
				(o, e) => parameters.Viscosity = (double)viscosityInput.Value;
			viscosityInput.Minimum = 0;

			var emptyLabel = MakeAndAddLabel("", new());

			this.mainGrid.Children.Add(this.personsCanvas);
			Grid.SetColumn(personsCanvas, 0);
			Grid.SetRow(personsCanvas, 1);

			this.personsCanvas.Background = personsCanvasBackBrush;
			this.personsCanvas.SizeChanged += ResizePersonsPanel;

			Canvas.SetLeft(indicatorsCanvas, 0);
			Canvas.SetTop(indicatorsCanvas, 0);
			Panel.SetZIndex(indicatorsCanvas, 1);
			this.indicatorsCanvas.Orientation = Orientation.Vertical;
			this.indicatorsCanvas.Background = Brushes.Transparent;

			this.indicatorN = MakeAndAddIndicator("N");
			this.indicatorTemperature = MakeAndAddIndicator("T");
			this.indicatorPressure = MakeAndAddIndicator("P");
			this.indicatorTimeQuant = MakeAndAddIndicator("Q");

			this.Width = parameters.MaxWidth;
			this.Height = parameters.MaxHeight;
			this.Closed += (o, e) => Stop();

			#region hide
			if (true)
			{
				gLabel.Visibility = Visibility.Collapsed;
				gInput.Visibility = Visibility.Collapsed;
				gDeltaLabel.Visibility = Visibility.Collapsed;
				gDeltaInput.Visibility = Visibility.Collapsed;
				elasticityLabel.Visibility = Visibility.Collapsed;
				elasticityInput.Visibility = Visibility.Collapsed;
				viscosityLabel.Visibility = Visibility.Collapsed;
				viscosityInput.Visibility = Visibility.Collapsed;
			}
			#endregion
		}

		private Label MakeAndAddLabel(string text, Thickness uiMargin)
		{
			var label = new Label()
			{
				Content = text,
				HorizontalContentAlignment = HorizontalAlignment.Right,
				Background = backColor,
				Foreground = Brushes.White,
				Margin = uiMargin,
				BorderThickness = new Thickness(0),
				Height = basicH,
			};

			DockPanel.SetDock(label, Dock.Left);
			this.controlDockPanel.Children.Add(label);

			return label;
		}

		private DoubleUpDown MakeAndAddInput(double startValue, Thickness uiMargin, string ValueName = null)
		{
			var input = new DoubleUpDown()
			{
				Background = Brushes.White,
				Foreground = backColor,
				Margin = uiMargin,
				BorderThickness = new Thickness(0),
				Height = basicH,
				Width = 50,
				Value = startValue,
				DataContext = parameters,
				AllowSpin = true,
				AllowTextInput = false,
				AllowInputSpecialValues = AllowedSpecialValues.None,
				Focusable = false,
			};

			if (!string.IsNullOrEmpty(ValueName))
			{
				input.SetBinding(DoubleUpDown.ValueProperty, ValueName);
			}

			DockPanel.SetDock(input, Dock.Left);
			this.controlDockPanel.Children.Add(input);

			return input;
		}

		private Label MakeAndAddIndicator(string prefix)
		{
			var indicator = new Label()
			{
				Height = basicH,
				Background = Brushes.Transparent,
				Foreground = Brushes.Gray,
				FontSize = 20,
				Tag = prefix,
				Content = $"{prefix} = 0",
			};

			indicatorsCanvas.Children.Add(indicator);
			return indicator;
		}

		private void ResizePersonsPanel(object _, SizeChangedEventArgs e)
		{
			if (this.personsCanvas.ActualWidth > 0)
			{
				this.parameters.MaxWidth = (int)this.personsCanvas.ActualWidth;
			}

			if (this.personsCanvas.ActualHeight > 0)
			{
				this.parameters.MaxHeight = (int)this.personsCanvas.ActualHeight;
			}
		}
		#endregion

		#region start/stop
		private void Stop()
		{
			if (isStarted && iterationTimer is not null)
			{
				iterationTimer.Stop();
				isStarted = false;
			}
		}

		private void Continue()
		{
			if (!isStarted && iterationTimer is not null && persons.Any())
			{
				iterationTimer.Start();
				isStarted = true;
			}
		}

		private void New(object? _, EventArgs __)
		{
			if (isStarted)
			{
				Stop();
			}

			parameters.ResetTimeQuant();
			indicatorTimeQuant.UpdateIndicator(parameters.TimeQuantMseconds);
			SetupIterationTimer();
			SetupStatistics();

			lock (locker)
			{
				persons.Clear();
			}

			personsCanvas.Dispatcher.Invoke(() =>
			{
				personsCanvas.Children.Clear();
				personsCanvas.Children.Add(this.indicatorsCanvas);

				for (int i = 0; i < parameters.PersonsCount; i++)
				{
					MakeNewPerson();
				}
			});

			iterationTimer.Start();
			isStarted = true;
		}

		private void OnIteration(object _, ElapsedEventArgs __)
		{
			if (canIterate)
			{
				canIterate = false;
				var currentDuration = Iterate();
				statistics.TimeQuant = currentDuration;
				canIterate = true;
			}
		}

		private double Iterate()
		{
			var sw = new Stopwatch();
			sw.Start();

			if (persons.Count <= 1)
			{
				New(null, null);
				return 0;
			}

			Person.Iteration(persons, deads, newBorns, locker, personsCanvas, statistics);
			var rnd = new Random();

			//смерть
			while (deads.TryTake(out Person person))
			{
				double count = persons.Count + deads.Count;
				double p = count / (parameters.PersonsCount + count);

				if (rnd.NextDouble() > p - 0.2)//с доп.поправкой, чтобы увеличить верояность смерти
				{
					lock (locker)
					{
						persons.Remove(person);
					}
					person.StartDying(personsCanvasBackBrush.Color);
				}
			}

			//зарождение новых
			while (newBorns.TryTake(out Point point))
			{
				double count = persons.Count + newBorns.Count;
				double p = count / (parameters.PersonsCount + count);

				if (rnd.NextDouble() < p)
				{
					this.Dispatcher.Invoke(() =>
					{
						MakeNewPerson().SetLocation(point);
					});
				}
			}

			//не применяем автонастройку gDelta, если пользователь руками её изменил, до тех пор, пока не изменится число persons
			canAutoChangeGdelta = canAutoChangeGdelta || persons.Count != lastPersonsCount;

			if (canAutoChangeGdelta)
			{
				var newCountDiff = (parameters.PersonsCount - persons.Count) / ((double)parameters.PersonsCount + persons.Count);
				var newGdelta = newCountDiff * parameters.G;
				parameters.Gdelta = newGdelta;
			}

			sw.Stop();
			return sw.ElapsedMilliseconds;
		}

		private Person MakeNewPerson()
		{
			var person = new Person(parameters);

			lock (locker)
			{
				persons.Add(person);
			}

			personsCanvas.Dispatcher.Invoke(() => personsCanvas.Children.Add(person.MainCircleCanvas));

			person.Strike +=
				(o, e) => soundPlayers[person.ChromeStep].Play();

			person.Kill +=
				(o, e) => personsCanvas.Dispatcher.Invoke(
					() => personsCanvas.Children.Remove(person.MainCircleCanvas));

			return person;
		}

		private void StartContinue(object? _, EventArgs __)
		{
			if (isStarted)
			{
				Stop();
			}
			else
			{
				Continue();
			}
		}
		#endregion
	}
}
