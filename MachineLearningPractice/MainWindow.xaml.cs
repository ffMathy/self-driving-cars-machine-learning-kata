using MachineLearningPractice.Helpers;
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
        private readonly DirectionHelper directionHelper;

        private const int Spacing = 50;

        public MainWindow()
        {
            this.random = new Random();
            this.directionHelper = new DirectionHelper(this.random);

            InitializeComponent();
            GenerateNewMap();
        }

        private void GenerateNewMapButton_Click(object sender, RoutedEventArgs e)
        {
            GenerateNewMap();
        }

        private void GenerateNewMap()
        {
            MapCanvas.Children.Clear();

            var mapGeneratorService = new MapGeneratorService(
                this.random,
                this.directionHelper);
            var mapNodes = mapGeneratorService.PickRandomPredefinedMap();

            foreach (var node in mapNodes)
            {
                AddCircle(node);

                foreach (var line in node.Lines)
                {
                    AddLine(line);
                }
            }
        }

        private void AddCircle(Models.MapNode node)
        {
            const int nodeSize = 10;
            var ellipse = new Ellipse()
            {
                Width = nodeSize,
                Height = nodeSize,
                Stroke = Brushes.Gray,
                StrokeThickness = 1
            };
            MapCanvas.Children.Add(ellipse);

            Canvas.SetLeft(ellipse, node.Point.X * Spacing - nodeSize / 2);
            Canvas.SetTop(ellipse, node.Point.Y * Spacing - nodeSize / 2);
        }

        private void AddLine(Models.Line line)
        {

            MapCanvas.Children.Add(new Line()
            {
                X1 = line.Start.X * Spacing,
                Y1 = line.Start.Y * Spacing,
                X2 = line.End.X * Spacing,
                Y2 = line.End.Y * Spacing,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            });
        }
    }
}
