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

        private const int Spacing = 100;

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
            var simulations = new List<CarSimulation>();
            for(var i=0;i<50;i++)
            {
                simulations.Add(new CarSimulation(
                    random,
                    map,
                    carNeuralNetwork,
                    0.1));
            }

            var hasCrashed = false;
            while(!hasCrashed)
            {
                ClearCanvas();
                RenderMap();

                foreach(var simulation in simulations)
                {
                    RenderCar(simulation.Car);

                    simulation.Tick();
                }

                await Task.Delay(1000);

                break;
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

                foreach (var line in node.Lines)
                {
                    RenderLine(line);
                }
            }
        }

        private void RenderCar(Car car)
        {
            const int nodeSize = Spacing / 3;

            var rectangle = new Rectangle()
            {
                Width = nodeSize,
                Height = nodeSize,
                Fill = Brushes.Transparent,
                Stroke = Brushes.Gray,
                StrokeThickness = 1,
                Opacity = 1
            };
            MapCanvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, car.BoundingBox.Location.X * Spacing - nodeSize / 2);
            Canvas.SetTop(rectangle, car.BoundingBox.Location.Y * Spacing - nodeSize / 2);
        }

        private void RenderMapNode(Models.MapNode node)
        {
            const int nodeSize = Spacing;

            var rectangle = new Rectangle()
            {
                Width = nodeSize,
                Height = nodeSize,
                Fill = Brushes.White,
                Opacity = 1
            };
            MapCanvas.Children.Add(rectangle);

            Canvas.SetLeft(rectangle, node.Position.X * Spacing - nodeSize / 2);
            Canvas.SetTop(rectangle, node.Position.Y * Spacing - nodeSize / 2);
        }

        private void RenderLine(Models.Line line)
        {
            MapCanvas.Children.Add(new System.Windows.Shapes.Line()
            {
                X1 = line.Start.X * Spacing,
                Y1 = line.Start.Y * Spacing,
                X2 = line.End.X * Spacing,
                Y2 = line.End.Y * Spacing,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            });
        }
    }
}
