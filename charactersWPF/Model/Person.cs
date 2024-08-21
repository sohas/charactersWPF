using charactersWPF.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

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
            private double rotateAngle;

            private readonly List<double> characters = new();
            private readonly List<Color> CharactersColors = new();
            private readonly Color meanColor;
            private Ellipse mainCircle;
            private Canvas mainCircleCanvas;
            private readonly double basicRotateAngle;
            #endregion

            #region public properties
            public Ellipse MainCircle => mainCircle;
            public Canvas MainCircleCanvas => mainCircleCanvas;
            public bool IsCaptured
            {
                  get; set;
            }
            public Point CaptureDiff
            {
                  get; set;
            }

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

            public double RotateAngle
            {
                  get
                  {
                        return rotateAngle;
                  }
                  set
                  {
                        if (rotateAngle != value)
                        {
                              rotateAngle = value;
                              NotifyPropertyChanged("RotateAngle");
                        }
                  }
            }

            public int ChromeStep { get; private set; }
            public double ChromePower { get; private set; }

            #endregion
            public event EventHandler Strike;
            public event PropertyChangedEventHandler? PropertyChanged;

            public Person(BasicParameters parameters)
            {
                  Parameters ??= parameters;

                  var rnd = new Random();
                  SetCharecters(rnd);
                  SetStartKinematic(rnd);
                  CharactersColors = characters.Select(x => GetColor(x)).ToList();
                  meanColor = GetMeanColor(CharactersColors);
                  SetCircles();
                  basicRotateAngle = rnd.NextDouble() * 5;
            }

            #region pubic methods
            public static void Iteration(List<Person> persons)
            {
                  for (var i = 0; i < persons.Count; i++)
                  {
                        var first = persons[i];

                        if (first.IsCaptured)
                        {
                              continue;
                        }

                        first.forceX = 0;
                        first.forceY = 0;

                        for (var j = 0; j < persons.Count; j++)
                        {
                              if (j == i)
                              {
                                    continue;
                              }

                              var second = persons[j];
                              first.SetForceFromSecondPerson(second);
                        }

                        first.SetNewVelocity();
                        first.AddViscosity();
                        first.SetNewLocation();
                  }

                  foreach (var person in persons)
                  {
                        person.X_LeftOnCanvas = person.newX;
                        person.Y_TopOnCanvas = person.newY;
                        person.AddWallsReaction();
                        person.Rotate();
                  }
            }

            public void SetLocation(Point location)
            {
                  X_LeftOnCanvas = newX = location.X;
                  Y_TopOnCanvas = newY = location.Y;
            }
            #endregion

            private void NotifyPropertyChanged(string v)
            {
                  PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
            }

            private void SetCharecters(Random rnd)
            {
                  var charactersNumber = rnd.Next(1, Parameters.MaxNumberCharacters + 1);

                  double xAccum = 0;
                  double yAccum = 0;

                  for (var i = 0; i < charactersNumber; i++)
                  {
                        var characterIndex = rnd.Next(0, Parameters.MaxNumberCharacterTypes);
                        var character = characterIndex / (double)Parameters.MaxNumberCharacterTypes;//от 0 до 1 исключая 1
                        characters.Add(character);

                        var angle = Math.PI * 2 * character;
                        xAccum += Math.Cos(angle);
                        yAccum += Math.Sin(angle);
                  }

                  var r = Math.Sqrt(xAccum * xAccum + yAccum * yAccum);
                  var ang =
                          r == 0 ?
                          0 :
                          yAccum >= 0 ?
                          Math.Acos(xAccum / r) :
                          Math.PI * 2 - Math.Acos(xAccum / r);

                  ChromePower = r / charactersNumber;
                  ChromeStep = (int)(ang / (Math.PI * 2) * Parameters.MaxNumberCharacterTypes);
                  ChromeStep = ChromeStep % 2 == 0 ? ChromeStep / 2 * 7 : (ChromeStep - 1) / 2 * 7 + 4;
                  ChromeStep %= 24;

                  characters.Sort();
            }



            private void SetStartKinematic(Random rnd)
            {
                  this.velocityX = 0;
                  this.velocityY = 0;
                  this.X_LeftOnCanvas = rnd.NextDouble() * (Parameters.MaxWidth - Parameters.Radius * 2);
                  this.Y_TopOnCanvas = rnd.NextDouble() * (Parameters.MaxHeight - Parameters.Radius * 2);
            }

            private void SetCircles()
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

                  Binding bta = new Binding("RotateAngle");
                  bta.Mode = BindingMode.TwoWay;
                  bta.Converter = new RotateConverter(Parameters.Radius, Parameters.Radius);
                  mainCircleCanvas.SetBinding(Canvas.RenderTransformProperty, bta);

                  mainCircle = new Ellipse()
                  {
                        Width = Parameters.Radius * 2,
                        Height = Parameters.Radius * 2,
                        Fill = new SolidColorBrush(meanColor),
                        Stroke = Brushes.White,
                        StrokeThickness = 0.3,
                        DataContext = this,
                  };

                  mainCircle.SetValue(Canvas.LeftProperty, 0.0);
                  mainCircle.SetValue(Canvas.TopProperty, 0.0);
                  mainCircleCanvas.Children.Add(mainCircle);

                  var sina0 = Math.Sin(Math.PI / Parameters.MaxNumberCharacters);
                  var d = 2 * Parameters.Radius * sina0 / (1 + sina0);

                  for (var i = 0; i < CharactersColors.Count; i++)
                  {
                        var a = i * 2 * Math.PI / CharactersColors.Count;

                        var ell = new Ellipse()
                        {
                              Width = d,
                              Height = d,
                              Fill = new SolidColorBrush(CharactersColors[i])
                        };

                        var dx = Parameters.Radius + (Parameters.Radius - d / 2) * Math.Sin(a) - d / 2;
                        var dy = Parameters.Radius - (Parameters.Radius - d / 2) * Math.Cos(a) - d / 2;
                        ell.SetValue(Canvas.LeftProperty, dx);
                        ell.SetValue(Canvas.TopProperty, dy);
                        mainCircleCanvas.Children.Add(ell);
                  }
            }

            private void Rotate()
            {
                  RotateAngle += basicRotateAngle;
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

            private static Color GetMeanColor(List<Color> colors)
            {
                  if (colors is null || !colors.Any())
                  {
                        return Colors.White;
                  }

                  var n = colors.Count;

                  double a = 0;
                  double r = 0;
                  double g = 0;
                  double b = 0;

                  foreach (var color in colors)
                  {
                        a += color.A;
                        r += color.R;
                        g += color.G;
                        b += color.B;
                  }

                  a /= n;
                  r /= n;
                  g /= n;
                  b /= n;

                  a = a > 255 ? 255 : a;
                  r = r > 255 ? 255 : r;
                  g = g > 255 ? 255 : g;
                  b = b > 255 ? 255 : b;

                  return Color.FromArgb((byte)(2 * a / 3), (byte)r, (byte)g, (byte)b);
            }

            private void SetForceFromSecondPerson(Person second)
            {
                  var distanceX = second.X_LeftOnCanvas - this.X_LeftOnCanvas;
                  var distanceY = second.Y_TopOnCanvas - this.Y_TopOnCanvas;
                  var distanceSQ = distanceX * distanceX + distanceY * distanceY;
                  var distance = Math.Sqrt(distanceSQ);
                  double basicForceSum = 0;

                  if (distance < 2 * Parameters.Radius)
                  {
                        basicForceSum -= Parameters.Elasticity;
                        Strike?.Invoke(this, EventArgs.Empty);
                  }
                  else
                  {
                        foreach (var character in this.characters)
                        {
                              foreach (var secondCharacter in second.characters)
                              {
                                    var addForce = character.GetBasicForceFrom(secondCharacter, Parameters);
                                    basicForceSum += addForce / (1 + distanceSQ / (Parameters.Dimention * Parameters.Dimention));
                              }
                        }
                  }

                  var force = basicForceSum;

                  if (distance == 0)
                  {
                        distance = distanceX = distanceY = 1;
                  }

                  forceX += (distanceX / distance) * force;
                  forceY += (distanceY / distance) * force;
            }

            private void AddViscosity()
            {
                  var vsqr = (velocityX * velocityX + velocityY * velocityY);
                  var visc = Parameters.Viscosity / (Parameters.Dimention * Parameters.Dimention * Parameters.Dimention * Parameters.Dimention);

                  velocityX /= (1 + vsqr * visc);
                  velocityY /= (1 + vsqr * visc);
            }

            private void AddWallsReaction()
            {
                  if (X_LeftOnCanvas < 0 && velocityX <= 0)
                  {
                        velocityX = -velocityX;
                        X_LeftOnCanvas = 3;
                        Strike?.Invoke(this, EventArgs.Empty);
                  }
                  else if (X_LeftOnCanvas > Parameters.MaxWidth - Parameters.Radius * 2 && velocityX >= 0)
                  {
                        velocityX = -velocityX;
                        X_LeftOnCanvas = Parameters.MaxWidth - Parameters.Radius * 2 - 3;
                        Strike?.Invoke(this, EventArgs.Empty);
                  }

                  if (Y_TopOnCanvas < 0 && velocityY <= 0)
                  {
                        velocityY = -velocityY;
                        Y_TopOnCanvas = 3;
                        Strike?.Invoke(this, EventArgs.Empty);
                  }
                  else if (Y_TopOnCanvas > Parameters.MaxHeight - Parameters.Radius * 2 && velocityY >= 0)
                  {
                        velocityY = -velocityY;
                        Y_TopOnCanvas = Parameters.MaxHeight - Parameters.Radius * 2 - 3;
                        Strike?.Invoke(this, EventArgs.Empty);
                  }
            }

            private void SetNewVelocity()
            {
                  velocityX += forceX * Parameters.TimeQuant / Parameters.Dimention;
                  velocityY += forceY * Parameters.TimeQuant / Parameters.Dimention;
            }

            private void SetNewLocation()
            {
                  newX = this.X_LeftOnCanvas + (velocityX * Parameters.TimeQuant) / Parameters.Dimention;
                  newY = this.Y_TopOnCanvas + (velocityY * Parameters.TimeQuant) / Parameters.Dimention;
            }
      }
}