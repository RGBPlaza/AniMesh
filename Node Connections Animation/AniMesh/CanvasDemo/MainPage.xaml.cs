using Constellations;
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
using Windows.UI.ViewManagement;
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
        bool isPlaying = true;

        int nodeCount = 16;
        float speedMultiplyer = 10.0f;
        List<Node> nodes = new List<Node>();
        
        public MainPage()
        {
            this.InitializeComponent();

            MyCanvas.SizeChanged += MyCanvas_SizeChanged;
            NodeCountSlider.ValueChanged += NodeCountSlider_ValueChanged;
            SpeedSlider.ValueChanged += SpeedSlider_ValueChanged;
            Window.Current.Content.KeyDown += OnKeyDown;
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += MainPage_VisibleBoundsChanged;
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
            while (isPlaying)
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

                                    line.X2 = passingNode.Position.X + (passingNode.Diameter / 2);
                                    line.Y2 = passingNode.Position.Y + (passingNode.Diameter / 2);

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

        private void TogglePlaying()
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                PlayPauseButton.Content = new SymbolIcon(Symbol.Pause);

                stopwatch.Restart();
                Task.Run(UpdateLoop);
            }
            else
                PlayPauseButton.Content = new SymbolIcon(Symbol.Play);
        }

        private void SetControlVisibility(Visibility visibility)
        {
            TitleTextBlock.Visibility = visibility;
            NodeCountStackPanel.Visibility = visibility;
            SpeedStackPanel.Visibility = visibility;
            PlayPauseButton.Visibility = visibility;
            FullScreenButton.Visibility = visibility;
            CompactOverlayButton.Visibility = visibility;
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            if (ApplicationView.GetForCurrentView().TryEnterFullScreenMode())
            {
                SetControlVisibility(Visibility.Collapsed);
                Window.Current.CoreWindow.PointerCursor = null;
            }
        }

        private async void OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            if (e.Key == Windows.System.VirtualKey.Escape && applicationView.IsFullScreenMode)
                applicationView.ExitFullScreenMode();
            else if (e.Key == Windows.System.VirtualKey.Escape && applicationView.ViewMode == ApplicationViewMode.CompactOverlay)
            {
                MyCanvas.Margin = new Thickness(-16, -8, -16, -8);
                await applicationView.TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
            else if (e.Key == Windows.System.VirtualKey.Space)
                TogglePlaying();
        }
        
        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePlaying();
        }

        private void MainPage_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            if (!sender.IsFullScreenMode && sender.ViewMode != ApplicationViewMode.CompactOverlay)
            {
                SetControlVisibility(Visibility.Visible);
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
            }
            else
            {
                Focus(FocusState.Programmatic);
            }
        }

        private async void CompactOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            if (await applicationView.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay))
            {
                SetControlVisibility(Visibility.Collapsed); MyCanvas.Margin = new Thickness(32);
            }

        }
    }
}
