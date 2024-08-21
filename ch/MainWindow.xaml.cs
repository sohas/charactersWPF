using Characters.Model;
using System.Collections.Generic;
using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit;
using System.Windows.Shapes;
using NAudio.Wave;

namespace charactersWPF
{
        public partial class MainWindow : Window
        {
                private readonly Grid mainGrid = new();
                private readonly DockPanel controlDockPanel = new();
                private readonly Canvas personsCanvas = new();
                private readonly Button closeButton = new Button();
                private readonly Button fullButton = new Button();
                private readonly Button minimizeButton = new Button();
                private readonly Button newButton = new Button();
                private readonly Button startButton = new Button();

                private Person capturedPerson = null;
                private List<Person> persons = new();
                private Timer timer = new Timer();
                private bool isStarted = false;
                private BasicParameters parameters;
                private Brush lastPersonBrush;
                private System.Windows.WindowState currentState = System.Windows.WindowState.Normal;
                private Brush backColor;

                private MediaPlayer mp;
                private Uri uri;


                public MainWindow()
                {
                        var size = new Size(600, 600);
                        var radius = 20;
                        var maxNumberCharacterTypes = 5;
                        var maxNumberCharacters = 1;
                        var personsCount = 100;
                        var gPlus = 10;
                        var gMinus = 12;
                        var elasticity = 0;
                        var viscosity = 0;
                        var timeQuant = 10;

                        parameters = new BasicParameters(
                            size, radius, maxNumberCharacterTypes, maxNumberCharacters, personsCount,
                            gPlus, gMinus, elasticity, viscosity, timeQuant);

                        MakeDesign();


                        //OpenFileDialog openFileDialog = new OpenFileDialog();
                        //openFileDialog.Filter = "MP3 files (*.wav)|*.wav|All files (*.*)|*.*";
                        //if (openFileDialog.ShowDialog() == true)
                        //{
                                var t = CharactersResources.Alarm01;


                                //uri = new Uri(openFileDialog.FileName, UriKind.RelativeOrAbsolute);
                                //var fs = new FileStream(openFileDialog.FileName, FileMode.Open);
                                var reader = new WaveFileReader(t); // Mp3FileReader
                                WaveOut wout = new WaveOut();
                                wout.Init(reader);
                                wout.Play();
                        //}
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
                        DockPanel.SetDock(closeButton, Dock.Right);
                        this.closeButton.Content = " X ";
                        this.closeButton.Width = 25;
                        this.closeButton.Click += (o, e) => Close();
                        this.controlDockPanel.Children.Add(closeButton);

                        this.fullButton.Background = backColor;
                        this.fullButton.Foreground = Brushes.White;
                        this.fullButton.Margin = new(0);
                        this.fullButton.BorderThickness = new Thickness(0);
                        this.fullButton.HorizontalAlignment = HorizontalAlignment.Right;
                        DockPanel.SetDock(fullButton, Dock.Right);
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

                        this.controlDockPanel.Children.Add(fullButton);

                        this.minimizeButton.Background = backColor;
                        this.minimizeButton.Foreground = Brushes.White;
                        this.minimizeButton.Visibility = Visibility.Collapsed;
                        this.minimizeButton.Margin = new(0);
                        this.minimizeButton.BorderThickness = new Thickness(0);
                        this.minimizeButton.HorizontalAlignment = HorizontalAlignment.Right;
                        DockPanel.SetDock(minimizeButton, Dock.Right);
                        this.minimizeButton.Content = "__";
                        this.minimizeButton.Width = 25;
                        this.minimizeButton.Click += (o, e) => this.WindowState = System.Windows.WindowState.Minimized;
                        this.controlDockPanel.Children.Add(minimizeButton);

                        var uiMargin = new Thickness(0, 0, 10, 0);

                        this.startButton.Background = backColor;
                        this.startButton.Foreground = Brushes.White;
                        this.startButton.Margin = uiMargin;
                        this.startButton.BorderThickness = new Thickness(0);
                        this.startButton.Content = "Start/Pause";
                        this.startButton.Click += StartContinue;
                        this.controlDockPanel.Children.Add(newButton);

                        var basicH = startButton.Height;

                        this.newButton.Background = backColor;
                        this.newButton.Foreground = Brushes.White;
                        this.newButton.Margin = uiMargin;
                        this.newButton.BorderThickness = new Thickness(0);
                        this.newButton.Height = basicH;
                        this.newButton.Content = "New";
                        this.newButton.Click += New;
                        this.controlDockPanel.Children.Add(startButton);

                        var chTypesNumberLabel = MakeAndAddLabel("T", basicH, new());

                        var chTypesNumberInput = MakeAndAddInput(parameters.MaxNumberCharacterTypes, basicH, uiMargin);
                        chTypesNumberInput.ValueChanged += (o, e) => parameters.MaxNumberCharacterTypes = (int)chTypesNumberInput.Value;

                        var chNumberLabel = MakeAndAddLabel("Ch", basicH, new());

                        var chNumberInput = MakeAndAddInput(parameters.MaxNumberCharacters, basicH, uiMargin);
                        chNumberInput.ValueChanged += (o, e) => parameters.MaxNumberCharacters = (int)chNumberInput.Value;

                        var personsNumberLabel = MakeAndAddLabel("N", basicH, new());

                        var personsNumberInput = MakeAndAddInput(parameters.PersonsCount, basicH, uiMargin);
                        personsNumberInput.ValueChanged += (o, e) => parameters.PersonsCount = (int)personsNumberInput.Value;

                        var gPlusLabel = MakeAndAddLabel("G+", basicH, new());

                        var gPlusInput = MakeAndAddInput((int)parameters.Gplus, basicH, uiMargin);
                        gPlusInput.ValueChanged += (o, e) => parameters.Gplus = (double)gPlusInput.Value;

                        var gMinusLabel = MakeAndAddLabel("G-", basicH, new());

                        var gMinusInput = MakeAndAddInput((int)parameters.Gminus, basicH, uiMargin);
                        gMinusInput.ValueChanged += (o, e) => parameters.Gminus = (double)gMinusInput.Value;

                        var elasticityLabel = MakeAndAddLabel("E", basicH, new());

                        var elasticityInput = MakeAndAddInput((int)parameters.Elasticity, basicH, uiMargin);
                        elasticityInput.ValueChanged += (o, e) => parameters.Elasticity = (double)elasticityInput.Value;

                        var viscosityLabel = MakeAndAddLabel("V", basicH, new());

                        var viscosityInput = MakeAndAddInput((int)parameters.Viscosity, basicH, uiMargin);
                        viscosityInput.ValueChanged += (o, e) => parameters.Viscosity = (double)viscosityInput.Value;

                        var emptyLabel = MakeAndAddLabel("", basicH, new());


                        this.mainGrid.Children.Add(this.personsCanvas);
                        Grid.SetColumn(personsCanvas, 0);
                        Grid.SetRow(personsCanvas, 1);

                        this.personsCanvas.Background = Brushes.Black;
                        this.personsCanvas.SizeChanged += ResizePersonsPanel;
                        this.personsCanvas.MouseUp += ReleasePerson;
                        this.personsCanvas.MouseMove += MovePerson;

                        this.Width = parameters.MaxWidth;
                        this.Height = parameters.MaxHeight;
                }

                private Label MakeAndAddLabel(string text, double height, Thickness uiMargin)
                {
                        var label = new Label()
                        {
                                Content = text,
                                HorizontalContentAlignment = HorizontalAlignment.Right,
                                Background = backColor,
                                Foreground = Brushes.White,
                                Margin = uiMargin,
                                BorderThickness = new Thickness(0),
                                Height = height,
                        };

                        DockPanel.SetDock(label, Dock.Left);
                        this.controlDockPanel.Children.Add(label);

                        return label;
                }

                private IntegerUpDown MakeAndAddInput(int startValue, double height, Thickness uiMargin)
                {
                        var input = new IntegerUpDown()
                        {
                                Background = Brushes.White,
                                Foreground = backColor,
                                Margin = uiMargin,
                                BorderThickness = new Thickness(0),
                                Height = height,
                                Width = 50,
                                Value = startValue,
                                AllowSpin = true,
                        };

                        DockPanel.SetDock(input, Dock.Left);
                        this.controlDockPanel.Children.Add(input);

                        return input;
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
                        if (isStarted && timer is not null)
                        {
                                timer.Stop();
                                isStarted = false;
                        }
                }

                private void Continue()
                {
                        if (!isStarted && timer is not null)
                        {
                                timer.Start();
                                isStarted = true;
                        }
                }

                private void New(object? _, EventArgs __)
                {
                        if (isStarted)
                        {
                                return;
                        }

                        persons.Clear();
                        personsCanvas.Children.Clear();

                        for (int i = 0; i < parameters.PersonsCount; i++)
                        {
                                var person = new Person(parameters);
                                persons.Add(person);
                                personsCanvas.Children.Add(person.MainCircleCanvas);
                                person.MainCircle.MouseDown += CapturePerson;
                        }

                        timer = new Timer();
                        timer.Interval = parameters.TimeQuant;
                        timer.Elapsed += (o, e) =>
                        {
                                Person.Iteration(persons);
                        };

                        timer.Start();
                        isStarted = true;
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

                #region move person
                private void CapturePerson(object o, MouseButtonEventArgs e)
                {
                        if (o is FrameworkElement fe && fe.DataContext is Person person)
                        {
                                person.IsCaptured = true;
                                person.CaptureDiff = new Point(
                                    e.MouseDevice.GetPosition(personsCanvas).X - person.X_LeftOnCanvas,
                                    e.MouseDevice.GetPosition(personsCanvas).Y - person.Y_TopOnCanvas);
                                capturedPerson = person;

                                if (o is Shape shape)
                                {
                                        lastPersonBrush = shape.Fill;
                                        shape.Fill = Brushes.LightGray;
                                }



                        }
                }

                private void MovePerson(object _, MouseEventArgs e)
                {
                        if (capturedPerson is null || e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
                        {
                                return;
                        }

                        var mx = e.MouseDevice.GetPosition(personsCanvas).X;
                        var my = e.MouseDevice.GetPosition(personsCanvas).Y;

                        var dx = capturedPerson.CaptureDiff.X;
                        var dy = capturedPerson.CaptureDiff.Y;

                        capturedPerson.SetLocation(new Point(mx - dx, my - dy));
                }

                private void ReleasePerson(object o, MouseButtonEventArgs e)
                {
                        if (capturedPerson is not null)
                        {
                                capturedPerson.MainCircle.Fill = lastPersonBrush;
                                capturedPerson.IsCaptured = false;
                                capturedPerson = null;

                                mp.Stop();
                        }
                }
                #endregion
        }
}