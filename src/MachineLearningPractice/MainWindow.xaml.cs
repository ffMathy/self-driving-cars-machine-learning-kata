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
            const int simulationCount = 1;

            var simulations = new List<CarSimulation>();
            for (var i = 0; i < simulationCount; i++)
            {
                simulations.Add(new CarSimulation(
                    random,
                    map,
                    carNeuralNetwork,
                    0.1));
            }

            var hasCrashed = false;
            while (!hasCrashed)
            {
                ClearCanvas();
                RenderMap();

                foreach (var simulation in simulations)
                {
                    RenderCar(simulation.Car);

                    if (!simulation.Tick())
                        hasCrashed = true;
                }

                await Task.Delay(50);
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

        private void RenderCar(Car car)
        {
            var width = car.BoundingBox.Size.Width;
            var height = car.BoundingBox.Size.Height;

            var rectangle = new Rectangle()
            {
                Width = width,
                Height = height,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Blue,
                StrokeThickness = 1,
                Opacity = 1,
                LayoutTransform = new RotateTransform(car.Angle)
            };
            MapCanvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, car.BoundingBox.Center.X - width / 2);
            Canvas.SetTop(rectangle, car.BoundingBox.Center.Y - height / 2);

            var sensorReadings = car.GetSensorReadings(map);
            var sensorDistances = new[]
            {
                sensorReadings.LeftSensorDistanceToWall,
                sensorReadings.CenterSensorDistanceToWall,
                sensorReadings.RightSensorDistanceToWall
            };

            var sensorLabels = sensorDistances
                .Select(x => Math.Round(x))
                .Select(x => x.ToString());

            var label = new TextBlock()
            {
                Text = sensorLabels.Aggregate((a, b) => a + "\n" + b),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            MapCanvas.Children.Add(label);

            Canvas.SetLeft(rectangle, car.BoundingBox.Center.X - width / 2);
            Canvas.SetTop(rectangle, car.BoundingBox.Center.Y - height / 2);
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
                Stroke = Brushes.Black,
                StrokeThickness = 2
            });
        }
    }
}
