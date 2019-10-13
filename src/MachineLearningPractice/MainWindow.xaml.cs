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

        private bool keepRunning;
        private ulong tick;

        private Map map;
        private List<CarSimulation> simulations;

        private CarSimulation bestSimulationInGeneration;

        public MainWindow()
        {
            this.random = new Random();
            this.carNeuralNetwork = new CarNeuralNetwork();
            this.directionHelper = new DirectionHelper(this.random);

            InitializeComponent();
            GenerateNewMap();
            LoadSimulations();
        }

        private void LoadSimulations()
        {
            const int simulationCount = 30;

            simulations = new List<CarSimulation>();
            for (var i = 0; i < simulationCount; i++)
            {
                simulations.Add(new CarSimulation(
                    random,
                    map,
                    carNeuralNetwork,
                    10m));
            }
        }

        private void GenerateNewMapButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateNewMap();
        }

        private async void TrainGenerationButton_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                await TrainGeneration(10);
            } while (keepRunning);
        }

        private async Task TrainGeneration(int tickDelay)
        {
            await RunSingleGeneration(simulations, tickDelay);
            TrainPendingInstructionsFromBestGeneration();
        }

        private void TrainPendingInstructionsFromBestGeneration()
        {
            foreach (var pendingTrainingInstruction in bestSimulationInGeneration.PendingTrainingInstructions)
            {
                carNeuralNetwork.Record(
                    pendingTrainingInstruction.CarSensorReading,
                    pendingTrainingInstruction.CarResponse);
            }

            carNeuralNetwork.Train();
        }

        private async Task RunSingleGeneration(List<CarSimulation> simulations, int tickDelay)
        {
            foreach (var simulation in simulations)
            {
                simulation.Reset();
            }

            var crashedCount = 0;
            while (crashedCount < simulations.Count)
            {
                tick++;

                if (tickDelay > 0)
                {
                    ClearCanvas();
                    RenderMap();
                }

                foreach (var simulation in simulations)
                {
                    if (!simulation.IsCrashed)
                    {
                        simulation.Tick();
                        if (simulation.IsCrashed)
                            crashedCount++;
                    }

                    if (tickDelay > 0)
                        RenderCarSimulation(simulation);

                    var carCurrentMapNode = simulation.CurrentMapNode;
                    var bestCurrentMapNode = bestSimulationInGeneration?.CurrentMapNode;

                    if (bestCurrentMapNode == null)
                    {
                        bestSimulationInGeneration = simulation;
                        continue;
                    }

                    if (carCurrentMapNode.Offset == bestCurrentMapNode.Offset + 1)
                    {
                        bestSimulationInGeneration = simulation;
                    }
                }

                if (tickDelay > 0)
                    await Task.Delay(tickDelay);
            }
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

            var color = Brushes.Green;
            if (carSimulation == bestSimulationInGeneration)
            {
                color = Brushes.Blue;
            }
            else if (carSimulation.IsCrashed)
            {
                color = Brushes.Red;
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
            MapCanvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, (double)car.BoundingBox.Location.X);
            Canvas.SetTop(ellipse, (double)car.BoundingBox.Location.Y);

            RenderCarSimulationSensorReadings(carSimulation);
        }

        private void RenderCarSimulationSensorReadings(CarSimulation carSimulation)
        {
            if (carSimulation.IsCrashed)
                return;

            var car = carSimulation.Car;

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
            MapCanvas.Children.Add(line);

            var sensorReadings = carSimulation.GetSensorReadings();
            var sensorReadingsArray = new[]
            {
                sensorReadings.LeftSensor,
                sensorReadings.CenterSensor,
                sensorReadings.RightSensor
            };

            RenderCarSimulationSensorReadings(carSimulation, sensorReadingsArray);
        }

        private void RenderCarSimulationSensorReadings(CarSimulation carSimulation, CarSensorReading?[] sensorReadingsArray)
        {
            if (carSimulation.IsCrashed)
                return;

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

            Canvas.SetLeft(rectangle, (double)node.Position.X - Map.TileSize / 2);
            Canvas.SetTop(rectangle, (double)node.Position.Y - Map.TileSize / 2);

            foreach (var line in node.ProgressLines)
                RenderLine(line, Brushes.LightGray, 0.25);

            foreach (var line in node.WallLines)
                RenderLine(line, Brushes.DimGray, 1);
        }

        private void RenderLine(Models.Line line, Brush brush, double opacity)
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
        }

        private async void TrainMultipleGenerationsButton_Click(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 50; i++)
            {
                await TrainGeneration(0);
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
