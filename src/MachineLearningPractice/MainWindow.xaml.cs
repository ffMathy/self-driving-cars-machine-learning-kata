using MachineLearningPractice.Helpers;
using MachineLearningPractice.Models;
using MachineLearningPractice.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private int generation;

        private Map map;

        private List<CarSimulation> simulations;
        private List<CarSimulation> currentBestSimulationsInGeneration;

        private HashSet<UIElement> renderedObjects;

        public MainWindow()
        {
            this.random = new Random();
            this.carNeuralNetwork = new CarNeuralNetwork();
            this.directionHelper = new DirectionHelper(this.random);
            this.currentBestSimulationsInGeneration = new List<CarSimulation>();
            this.renderedObjects = new HashSet<UIElement>();

            InitializeComponent();
            GenerateNewMap();
            LoadSimulations();
        }

        private void LoadSimulations()
        {
            const int simulationCount = 100;

            simulations = new List<CarSimulation>();
            for (var i = 0; i < simulationCount; i++)
            {
                simulations.Add(new CarSimulation(
                    random,
                    map,
                    carNeuralNetwork,
                    i == 0 || generation > 0 ? 0m : 0.5m));
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
                await TrainGeneration(1);
            } while (keepRunning);
        }

        private async Task TrainGeneration(int tickDelay)
        {
            await RunSingleGeneration(simulations, tickDelay);
            TrainPendingInstructionsFromBestGeneration();
        }

        private void TrainPendingInstructionsFromBestGeneration()
        {
            var instructions = currentBestSimulationsInGeneration
                .SelectMany(x => x
                    .PendingTrainingInstructions
                    .Take(x.PendingTrainingInstructions.Count / currentBestSimulationsInGeneration.Count));

            foreach (var pendingTrainingInstruction in instructions)
            {
                carNeuralNetwork.Record(
                    pendingTrainingInstruction.CarSensorReading,
                    pendingTrainingInstruction.CarResponse);
            }

            carNeuralNetwork.Train();

            generation++;
        }

        private void ParallelizeForIf<T>(bool condition, IEnumerable<T> enumerable, Action<T> action)
        {
            if(condition) { 
                Parallel.ForEach(enumerable, action);
            } else
            {
                foreach(var item in enumerable)
                {
                    action(item);
                }
            }
        }

        private async Task RunSingleGeneration(List<CarSimulation> simulations, int tickDelay)
        {
            foreach (var simulation in simulations)
            {
                simulation.Reset();
            }

            var crashedCount = 0;
            var isSlow = generation % 10 == 1 && tickDelay > 0;

            ClearCanvas();

            var ticks = 0L;
            var stopwatch = Stopwatch.StartNew();
            while (crashedCount < simulations.Count)
            {
                ticks++;

                var shouldSkipRender = !isSlow && stopwatch.ElapsedMilliseconds < 10000;
                if (!shouldSkipRender)
                    ClearCanvas();

                ParallelizeForIf(shouldSkipRender, simulations, simulation => { 
                    if (!simulation.IsCrashed)
                    {
                        simulation.Tick();
                        if (simulation.IsCrashed)
                            crashedCount++;
                    }

                    if (!shouldSkipRender)
                        RenderCarSimulation(simulation, !isSlow);
                });

                var amountOfBestGenerationsToPick = simulations.Count / 10;

                currentBestSimulationsInGeneration = simulations
                    .OrderBy(x => x.Fitness)
                    .Take(amountOfBestGenerationsToPick)
                    .ToList();

                if (!shouldSkipRender)
                    await Task.Delay(tickDelay);
            }

            stopwatch.Stop();

            if (!isSlow)
            {
                foreach (var simulation in simulations)
                {
                    RenderCarSimulation(simulation, false);
                }

                await Task.Delay(100);
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
            foreach (var element in renderedObjects)
                MapCanvas.Children.Remove(element);

            renderedObjects.Clear();
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
            renderedObjects.Add(element);
            MapCanvas.Children.Add(element);
        }

        private void RenderCarSimulation(CarSimulation carSimulation, bool shouldUseFastRender)
        {
            var car = carSimulation.Car;

            var color = Brushes.Green;
            if (currentBestSimulationsInGeneration.Contains(carSimulation))
            {
                color = Brushes.Blue;
            }
            else if (carSimulation.RandomnessFactor == 0)
            {
                color = Brushes.Purple;
            }
            else if (carSimulation.IsCrashed)
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
                Width = Map.TileSize,
                Height = Map.TileSize,
                Fill = Brushes.White,
                Opacity = 1
            };
            MapCanvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, (double)node.Position.X - Map.TileSize / 2);
            Canvas.SetTop(rectangle, (double)node.Position.Y - Map.TileSize / 2);

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

        private async void TrainMultipleGenerationsButton_Click(object sender, RoutedEventArgs e)
        {
            for (var i = 0; i < 3; i++)
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
