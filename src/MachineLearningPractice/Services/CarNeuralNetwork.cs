using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Statistics.Kernels;
using Accord.Statistics.Models.Regression.Linear;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Services
{
    public struct CarSimulationTick
    {
        public CarSensorReadingSnapshot CarSensorReading { get; set; }
        public CarResponse CarResponse { get; set; }
    }

    public class CarNeuralNetwork
    {
        const double MutationProbability = 0.2;

        private readonly IList<CarSimulationTick> trainingInstructions;

        private readonly ActivationNetwork network;
        private readonly BackPropagationLearning teacher;

        private readonly Random random;

        private bool hasTrained;

        public CarNeuralNetwork(
            Random random,
            ActivationNetwork network = null)
        {
            this.trainingInstructions = new List<CarSimulationTick>();

            this.random = random;

            this.hasTrained = network != null;

            //this.network = network ?? new ActivationNetwork(new ThresholdFunction(), 3, 4, 4, 2);
            this.network = network ?? new ActivationNetwork(new ThresholdFunction(), 3, 6, 2);
            this.teacher = new BackPropagationLearning(this.network);
        }

        public void Randomize()
        {
            network.Randomize();
        }

        public void Record(
            CarSensorReadingSnapshot sensorReading,
            CarResponse carResponse)
        {
            trainingInstructions.Add(new CarSimulationTick()
            {
                CarResponse = carResponse,
                CarSensorReading = sensorReading
            });
        }

        public CarNeuralNetwork CrossWith(CarNeuralNetwork other)
        {
            var a = CopyNeuralNetwork(this.network);
            var b = CopyNeuralNetwork(other.network);

            RandomSwap(ref a, ref b);
            CrossRandomAmountOfNeurons(a, b);

            return new CarNeuralNetwork(random, a);
        }

        private ActivationNetwork CopyNeuralNetwork(ActivationNetwork network)
        {
            using (var memoryStream = new MemoryStream())
            {
                network.Save(memoryStream);
                memoryStream.Position = 0;

                return (ActivationNetwork)Network.Load(memoryStream);
            }
        }

        private void CrossRandomAmountOfNeurons(
            ActivationNetwork a,
            ActivationNetwork b)
        {
            var aNetworkNeurons = GetNeuronsFromNeuralNetwork(a);
            var bNetworkNeurons = GetNeuronsFromNeuralNetwork(b);

            var slicePoint = random.Next(0, aNetworkNeurons.Count + 1);
            for (var i = slicePoint; i < aNetworkNeurons.Count; i++)
            {
                SwapNeuronBiases(
                    aNetworkNeurons,
                    bNetworkNeurons,
                    i);
            }
        }

        public void Mutate()
        {
            var neurons = GetNeuronsFromNeuralNetwork(network);
            foreach (var neuron in neurons)
            {
                if (random.NextDouble() >= MutationProbability)
                    continue;

                neuron.Threshold = MutateNeuronValue(neuron.Threshold);

                var weights = neuron.Weights;
                for (var i = 0; i < weights.Length; i++)
                {
                    weights[i] = MutateNeuronValue(weights[i]);
                }
            }
        }

        private double MutateNeuronValue(double value)
        {
            return value * (random.NextDouble() - 0.5) * 3 + (random.NextDouble() - 0.5);
        }

        private static void SwapNeuronBiases(
            IReadOnlyList<ActivationNeuron> aNetworkNeurons,
            IReadOnlyList<ActivationNeuron> bNetworkNeurons,
            int i)
        {
            var temporary = aNetworkNeurons[i].Threshold;
            aNetworkNeurons[i].Threshold = bNetworkNeurons[i].Threshold;
            bNetworkNeurons[i].Threshold = temporary;
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            var temporary = a;
            a = b;
            b = temporary;
        }

        private static IReadOnlyList<ActivationNeuron> GetNeuronsFromNeuralNetwork(ActivationNetwork network)
        {
            return network.Layers
                .SelectMany(x => x.Neurons)
                .Cast<ActivationNeuron>()
                .ToArray();
        }

        private void RandomSwap<T>(ref T a, ref T b)
        {
            if (random.NextDouble() <= 0.5)
                return;

            Swap(ref a, ref b);
        }

        public CarResponse Ask(
            CarSensorReadingSnapshot sensorReading)
        {
            if (!hasTrained)
            {
                return new CarResponse()
                {
                    AccelerationDeltaVelocity = (decimal)((random.NextDouble() - 0.5) / 1),
                    TurnDeltaAngle = (decimal)((random.NextDouble() - 0.5) / 1)
                };
            }

            var prediction = this.network.Compute(
                GetNeuralInputFromSensorReading(sensorReading));

            return new CarResponse()
            {
                AccelerationDeltaVelocity = NormalizeBinaryPrediction(prediction[0]),
                TurnDeltaAngle = NormalizeBinaryPrediction(prediction[1])
            };
        }

        private static decimal NormalizeBinaryPrediction(double prediction)
        {
            if (prediction > 0.66)
            {
                return 1m;
            }
            else if (prediction < 0.33)
            {
                return -1m;
            }

            return 0;
        }

        public void Train()
        {
            var inputs = this.trainingInstructions
                .Select(x => x.CarSensorReading)
                .Select(x => GetNeuralInputFromSensorReading(x))
                .ToArray();

            var outputs = this.trainingInstructions
                .Select(x => x.CarResponse)
                .Select(x =>
                {
                    return new[]
                    {
                        DenormalizeBinaryPrediction(x.AccelerationDeltaVelocity),
                        DenormalizeBinaryPrediction(x.TurnDeltaAngle)
                    };
                })
                .ToArray();

            teacher.RunEpoch(inputs, outputs);

            hasTrained = true;
        }

        private static double DenormalizeBinaryPrediction(decimal value)
        {
            if (value < 1)
            {
                return 0;
            }
            else if (value > 1)
            {
                return 1;
            }

            return 0.5;
        }

        private static double[] GetNeuralInputFromSensorReading(CarSensorReadingSnapshot sensorReading)
        {
            return new[] {
                sensorReading.CenterSensor?.Distance ?? 0,
                sensorReading.LeftSensor?.Distance ?? 0,
                sensorReading.RightSensor?.Distance ?? 0
            };
        }
    }
}
