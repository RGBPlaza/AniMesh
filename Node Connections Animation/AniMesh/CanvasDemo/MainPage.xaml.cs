using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CanvasDemo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static double canvasWidth = 0;
        public static double canvasHeight = 0;

        public Stopwatch stopwatch = new Stopwatch();

        int nodeCount = 16;
        float speedMultiplyer = 10.0f;
        List<Node> nodes = new List<Node>();
        
        public MainPage()
        {
            this.InitializeComponent();

            MyCanvas.SizeChanged += MyCanvas_SizeChanged;
            NodeCountSlider.ValueChanged += NodeCountSlider_ValueChanged;
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
        }

        public void InitaliseNodes()
        {
            for (int i = 0; i < nodeCount; i++)
            {
                Node node = new Node();
                nodes.Add(node);

                Ellipse ellipse = new Ellipse() { Width = node.Diameter, Height = node.Diameter, Fill = (Brush)Application.Current.Resources["SystemControlBackgroundBaseHighBrush"], Opacity = node.Diameter/6, Name = $"Node_{i}" };
                Canvas.SetLeft(ellipse, node.Position.X);
                Canvas.SetTop(ellipse, node.Position.Y);
                MyCanvas.Children.Add(ellipse);

                for (int j = i + 1; j < nodeCount; j++)
                {
                    Line line = new Line()
                    {
                        Stroke = (Brush)Application.Current.Resources["SystemControlBackgroundBaseHighBrush"],
                        StrokeThickness = 1,
                        Name = $"Line_{i}_{j}"
                    };

                    MyCanvas.Children.Add(line);
                }
            }
        }

        public void ClearNodes()
        {
            nodes.Clear();
            MyCanvas.Children.Clear();
        }

        private void MyCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            canvasWidth = e.NewSize.Width;
            canvasHeight = e.NewSize.Height;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            InitaliseNodes();

            stopwatch.Start();
            Task.Run(UpdateLoop);
        }

        public async Task UpdateLoop()
        {
            while (true)
            {
                float deltaTime = Math.Max(speedMultiplyer * stopwatch.ElapsedMilliseconds / 100, 0.5f);
                try
                {
                    for (int i = 0; i < nodeCount; i++)
                    {
                        Node currentNode = nodes[i];

                        currentNode.Update(deltaTime);
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            UIElement element = (UIElement)FindName($"Node_{i}");
                            if (element != null) {
                                Canvas.SetLeft(element, currentNode.Position.X);
                                Canvas.SetTop(element, currentNode.Position.Y);
                            }
                        });

                        for (int j = i + 1; j < nodeCount; j++)
                        {
                            Node passingNode = nodes[j];

                            double distance = currentNode.GetDistanceFrom(passingNode);

                            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {

                                Line line = (Line)FindName($"Line_{i}_{j}");

                                if (distance <= 360 && distance > 0)
                                {
                                    if (line == null)
                                    {
                                        line = new Line()
                                        {
                                            Stroke = (Brush)Application.Current.Resources["SystemControlBackgroundBaseHighBrush"],
                                            StrokeThickness = 1,
                                            Name = $"Line_{i}_{j}"
                                        };

                                        MyCanvas.Children.Add(line);
                                    }

                                    line.X1 = currentNode.Position.X + (currentNode.Diameter / 2);
                                    line.Y1 = currentNode.Position.Y + (currentNode.Diameter / 2);

                                    line.X2 = passingNode.Position.X + (currentNode.Diameter / 2);
                                    line.Y2 = passingNode.Position.Y + (currentNode.Diameter / 2);

                                    line.Opacity = (48 / distance) - 0.1;
                                }
                                else
                                {
                                    if (line != null)
                                        MyCanvas.Children.Remove(line);
                                }

                            });
                        }
                    }
                }
                catch { };

                stopwatch.Restart();
                await Task.Delay(20);
            }
        }

        private void NodeCountSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            NodeCountTextBlock.Text = $"Nodes: {e.NewValue}";

            ClearNodes();
            nodeCount = (int)e.NewValue;
            InitaliseNodes();
        }
        
        private void SpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            SpeedTextBlock.Text = $"Speed: {Math.Round((decimal)e.NewValue, 1, MidpointRounding.AwayFromZero)}";
            speedMultiplyer = (float)e.NewValue;
        }

    }

    public class Node
    {
        // Mechanics
        public Vector2 Dir;
        public Point Position;
        public double Velocity;

        // Visuals
        static Random rand = new Random();
        public double Diameter;

        public Node()
        {
            Position = new Point(rand.NextDouble() * MainPage.canvasWidth, rand.NextDouble() * MainPage.canvasHeight);
            Dir = new Vector2(rand.Next(-32, 33), rand.Next(-32, 33));
            Velocity = (rand.NextDouble() * 2) + 1;
            Diameter = (rand.NextDouble() * 2) + 4;
        }

        public void ResetMechanics()
        {
            int sideToStart = rand.Next(0, 4);
            if (sideToStart == 0)
            {
                Position = new Point(0, rand.NextDouble() * (MainPage.canvasHeight - Diameter));
                Dir = new Vector2(rand.Next(0, 33), rand.Next(-32, 33));
            }
            else if (sideToStart == 1)
            {
                Position = new Point(rand.NextDouble() * (MainPage.canvasWidth - Diameter), 0);
                Dir = new Vector2(rand.Next(-32, 33), rand.Next(0, 33));
            }
            else if (sideToStart == 2)
            {
                Position = new Point(MainPage.canvasWidth - Diameter, rand.NextDouble() * (MainPage.canvasHeight - Diameter));
                Dir = new Vector2(rand.Next(-32, 0), rand.Next(-32, 33));
            }
            else
            {
                Position = new Point(rand.NextDouble() * (MainPage.canvasWidth - Diameter), MainPage.canvasHeight - Diameter);
                Dir = new Vector2(rand.Next(-32, 33), rand.Next(-32, 0));
            }

            Velocity = (rand.NextDouble() * 2) + 1;
            Diameter = (rand.NextDouble() * 2) + 4;
        }

        public double GetDistanceFrom(Node node)
        {
            double dx = node.Position.X - Position.X;
            double dy = node.Position.Y - Position.Y;

            return Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
        }

        public void Update(double deltaTime)
        {
            Vector2 moveBy = Vector2.Multiply((float)(deltaTime * Velocity), (Vector2.Normalize(Dir)));
            Position.X += moveBy.X;
            Position.Y += moveBy.Y;

            if (Position.X < 0 || Position.X > MainPage.canvasWidth - Diameter || Position.Y < 0 || Position.Y > MainPage.canvasHeight - Diameter)
                ResetMechanics();
        }

    }

}
