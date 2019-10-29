using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using MachineLearningPractice.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MachineLearningPractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random random;
        private readonly CarsSimulation carsSimulation;
        private readonly DirectionHelper directionHelper;

        private bool keepRunning;

        private Map map;

        public MainWindow()
        {
            this.random = new Random();
            this.directionHelper = new DirectionHelper(this.random);

            InitializeComponent();
            GenerateNewMap();

            this.carsSimulation = new CarsSimulation(random, map);
        }

        private void GenerateNewMapButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateNewMap();
        }

        private void TrainGenerationButton_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                RunSingleGeneration(100);
            } while (keepRunning);
        }

        private static void DoEvents()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(f =>
                {
                    ((DispatcherFrame)f).Continue = false;
                    return null;
                }),
                frame);
            Dispatcher.PushFrame(frame);
        }

        private void RunSingleGeneration(int tickDelay)
        {
            var isSlow = carsSimulation.CurrentGeneration % 100 == 99 && tickDelay > 0;

            var stopwatch = Stopwatch.StartNew();

            carsSimulation.SimulateWholeGeneration(
                () =>
                {
                    var shouldSkipRender = !isSlow && stopwatch.ElapsedMilliseconds < 60_000;
                    if (shouldSkipRender)
                        return;

                    ClearCanvas();

                    foreach (var simulation in carsSimulation.AllSimulations)
                        RenderCarSimulation(simulation, shouldSkipRender);

                    Thread.Sleep(0);
                    DoEvents();
                },
                (timeElapsed) => Delay(isSlow ? tickDelay - timeElapsed : 0));

            stopwatch.Stop();

            Title = carsSimulation.CurrentGeneration + "";

            if (!isSlow)
            {
                ClearCanvas();

                foreach (var simulation in carsSimulation.AllSimulations)
                    RenderCarSimulation(simulation, false);

                if (tickDelay > 0)
                {
                    Delay(50);
                }
            }
        }

        private static void Delay(int durationInMilliseconds)
        {
            if(durationInMilliseconds == 0)
                return;

            var stopwatch = Stopwatch.StartNew();
            while(stopwatch.ElapsedMilliseconds < durationInMilliseconds)
            {
                Thread.Sleep(1);
                DoEvents();
            }

            stopwatch.Stop();
        }

        private void GenerateNewMap()
        {
            var mapGeneratorService = new MapGeneratorService(
                this.random,
                this.directionHelper);
            map = mapGeneratorService.PickRandomPredefinedMap();

            RenderMap();
        }

        private void ClearCanvas()
        {
            MapCanvas.Children.Clear();
            RenderMap();
        }

        private void RenderMap()
        {
            foreach (var node in map.Nodes)
            {
                RenderMapNode(node);
            }
        }

        private void Render(UIElement element)
        {
            MapCanvas.Children.Add(element);
        }

        private void RenderCarSimulation(CarSimulation carSimulation, bool shouldUseFastRender)
        {
            var car = carSimulation.Car;

            var color = Brushes.Green;
            if (carSimulation.IsCrashed)
            {
                color = Brushes.Red;

                if (shouldUseFastRender)
                    return;
            }

            var ellipse = new Ellipse()
            {
                Width = (double)car.BoundingBox.Size.Width,
                Height = (double)car.BoundingBox.Size.Height,
                Fill = Brushes.Transparent,
                Stroke = color,
                StrokeThickness = 3,
                Opacity = 1
            };
            Render(ellipse);

            Canvas.SetLeft(ellipse, (double)car.BoundingBox.Location.X);
            Canvas.SetTop(ellipse, (double)car.BoundingBox.Location.Y);

            if (carSimulation.IsCrashed)
                return;

            if (!shouldUseFastRender)
            {
                var line = new System.Windows.Shapes.Line()
                {
                    X1 = (double)car.BoundingBox.Center.X,
                    Y1 = (double)car.BoundingBox.Center.Y,
                    X2 = (double)car.BoundingBox.Center.X + ((double)car.ForwardDirectionLine.End.X * (double)car.BoundingBox.Size.Width),
                    Y2 = (double)car.BoundingBox.Center.Y + ((double)car.ForwardDirectionLine.End.Y * (double)car.BoundingBox.Size.Height),
                    Stroke = Brushes.Blue,
                    StrokeDashOffset = 2,
                    StrokeThickness = 2
                };
                Render(line);

                RenderCarSimulationSensorReadings(carSimulation);
            }
        }

        private void RenderCarSimulationSensorReadings(CarSimulation carSimulation)
        {
            return;

            if (carSimulation.IsCrashed)
                return;

            var sensorReadings = carSimulation.SensorReadings;
            var sensorReadingsArray = new[]
            {
                sensorReadings.LeftSensor,
                sensorReadings.CenterSensor,
                sensorReadings.RightSensor
            };

            var car = carSimulation.Car;
            foreach (var sensorReading in sensorReadingsArray)
            {
                if (sensorReading == null)
                    continue;

                var sensorLine = new System.Windows.Shapes.Line()
                {
                    X1 = (double)car.BoundingBox.Center.X,
                    Y1 = (double)car.BoundingBox.Center.Y,
                    X2 = (double)sensorReading.Value.IntersectionPoint.X,
                    Y2 = (double)sensorReading.Value.IntersectionPoint.Y,
                    Stroke = Brushes.Blue,
                    Opacity = 0.2,
                    StrokeThickness = 1
                };

                Render(sensorLine);
            }
        }

        private void RenderMapNode(MapNode node)
        {
            var rectangle = new Rectangle()
            {
                Width = (double)node.BoundingBox.Size.Width,
                Height = (double)node.BoundingBox.Size.Height,
                Fill = Brushes.White,
                Opacity = 1
            };
            MapCanvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, (double)node.BoundingBox.Location.X);
            Canvas.SetTop(rectangle, (double)node.BoundingBox.Location.Y);

            foreach (var progressLine in node.ProgressLines)
                RenderLine(progressLine.Line, Brushes.LightGray, 0.25, progressLine.Offset.ToString());

            foreach (var wallLine in node.WallLines)
                RenderLine(wallLine.Line, Brushes.DimGray, 1, null);
        }

        private void RenderLine(Models.Line line, Brush brush, double opacity, string annotation)
        {
            MapCanvas.Children.Add(new System.Windows.Shapes.Line()
            {
                X1 = (double)line.Start.X,
                Y1 = (double)line.Start.Y,
                X2 = (double)line.End.X,
                Y2 = (double)line.End.Y,
                Opacity = opacity,
                Stroke = brush,
                StrokeThickness = 2
            });

            if (annotation != null)
            {
                RenderTextBlock(line.Center, opacity, annotation);
            }
        }

        private void RenderTextBlock(Models.Point point, double opacity, string annotation)
        {
            var label = new TextBlock()
            {
                Text = annotation,
                FontSize = 10,
                Opacity = opacity,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };

            Canvas.SetLeft(label, (double)point.X);
            Canvas.SetTop(label, (double)point.Y);

            MapCanvas.Children.Add(label);
        }

        private void TrainMultipleGenerationsButton_Click(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 3; i++)
            {
                RunSingleGeneration(0);
            }

            TrainGenerationButton_Click(sender, e);
        }

        private void KeepRunningCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            keepRunning = true;
        }

        private void KeepRunningCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            keepRunning = false;
        }
    }
}
