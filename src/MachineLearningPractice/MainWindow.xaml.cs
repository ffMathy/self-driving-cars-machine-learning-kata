using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using MachineLearningPractice.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace MachineLearningPractice
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Random random;
        private readonly CarNeuralNetwork carNeuralNetwork;
        private readonly DirectionHelper directionHelper;

        private Map map;

        private CarSimulation bestSimulationInGeneration;

        public MainWindow()
        {
            this.random = new Random();
            this.carNeuralNetwork = new CarNeuralNetwork();
            this.directionHelper = new DirectionHelper(this.random);

            InitializeComponent();
            GenerateNewMap();
        }

        private void GenerateNewMapButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateNewMap();
        }

        private async void TrainGenerationButton_Click(object sender, RoutedEventArgs e)
        {
            await TrainGeneration();
        }

        private async Task TrainGeneration()
        {
            const int simulationCount = 10;

            var simulations = new List<CarSimulation>();
            for (var i = 0; i < simulationCount; i++)
            {
                simulations.Add(new CarSimulation(
                    random,
                    map,
                    carNeuralNetwork,
                    0.1));
            }

            var crashedCount = 0;
            while (crashedCount < simulations.Count)
            {
                ClearCanvas();
                RenderMap();

                foreach (var simulation in simulations)
                {
                    RenderCarSimulation(simulation);

                    if (simulation.IsCrashed)
                        continue;

                    if (!simulation.Tick())
                        crashedCount++;
                }

                await Task.Delay(100);
            }

            MessageBox.Show("Crashed!");
        }

        private void GenerateNewMap()
        {
            ClearCanvas();

            var mapGeneratorService = new MapGeneratorService(
                this.random,
                this.directionHelper);
            map = mapGeneratorService.PickRandomPredefinedMap();

            RenderMap();
        }

        private void ClearCanvas()
        {
            MapCanvas.Children.Clear();
        }

        private void RenderMap()
        {
            foreach (var node in map.Nodes)
            {
                RenderMapNode(node);
            }
        }

        private void RenderCarSimulation(CarSimulation carSimulation)
        {
            var car = carSimulation.Car;

            var ellipse = new Ellipse()
            {
                Width = car.BoundingBox.Size.Width,
                Height = car.BoundingBox.Size.Height,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Blue,
                StrokeThickness = 3,
                Opacity = 1
            };
            MapCanvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, car.BoundingBox.Location.X);
            Canvas.SetTop(ellipse, car.BoundingBox.Location.Y);

            RenderCarSimulationSensorReadings(carSimulation);
        }

        private void RenderCarSimulationSensorReadings(CarSimulation carSimulation)
        {
            var car = carSimulation.Car;

            var sensorReadings = carSimulation.GetSensorReadings();
            var sensorReadingsArray = new[]
            {
                sensorReadings.LeftSensor,
                sensorReadings.CenterSensor,
                sensorReadings.RightSensor
            };

            var sensorLabels = sensorReadingsArray
                .Where(x => x != null)
                .Select(x => Math.Round(x.Value.Distance))
                .Select(x => x + string.Empty);

            var label = new TextBlock()
            {
                Text = sensorLabels
                    .DefaultIfEmpty()
                    .Aggregate((a, b) => a + "\n" + b),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            MapCanvas.Children.Add(label);

            Canvas.SetLeft(label, car.BoundingBox.Center.X);
            Canvas.SetTop(label, car.BoundingBox.Center.Y);

            var line = new System.Windows.Shapes.Line()
            {
                X1 = car.BoundingBox.Center.X,
                Y1 = car.BoundingBox.Center.Y,
                X2 = car.BoundingBox.Center.X + (car.ForwardDirectionLine.End.X * car.BoundingBox.Size.Width),
                Y2 = car.BoundingBox.Center.Y + (car.ForwardDirectionLine.End.Y * car.BoundingBox.Size.Height),
                Stroke = Brushes.Blue,
                StrokeDashOffset = 2,
                StrokeThickness = 2
            };
            MapCanvas.Children.Add(line);

            RenderCarSimulationSensorReadings(car, sensorReadingsArray);
        }

        private void RenderCarSimulationSensorReadings(Car car, CarSensorReading?[] sensorReadingsArray)
        {
            foreach (var sensorReading in sensorReadingsArray)
            {
                if(sensorReading == null)
                    continue;

                var sensorLine = new System.Windows.Shapes.Line()
                {
                    X1 = car.BoundingBox.Center.X,
                    Y1 = car.BoundingBox.Center.Y,
                    X2 = sensorReading.Value.IntersectionPoint.X,
                    Y2 = sensorReading.Value.IntersectionPoint.Y,
                    Stroke = Brushes.Blue,
                    Opacity = 0.1,
                    StrokeThickness = 1
                };

                MapCanvas.Children.Add(sensorLine);
            }
        }

        private void RenderMapNode(MapNode node)
        {
            var rectangle = new Rectangle()
            {
                Width = Map.TileSize,
                Height = Map.TileSize,
                Fill = Brushes.White,
                Opacity = 1
            };
            MapCanvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, node.Position.X - Map.TileSize / 2);
            Canvas.SetTop(rectangle, node.Position.Y - Map.TileSize / 2);

            foreach (var line in node.Lines)
            {
                RenderLine(line);
            }
        }

        private void RenderLine(Models.Line line)
        {
            MapCanvas.Children.Add(new System.Windows.Shapes.Line()
            {
                X1 = line.Start.X,
                Y1 = line.Start.Y,
                X2 = line.End.X,
                Y2 = line.End.Y,
                Stroke = Brushes.DarkGray,
                StrokeThickness = 2
            });
        }
    }
}
