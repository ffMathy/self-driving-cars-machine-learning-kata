using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MachineLearningPractice.Services
{
    struct TrainingInstruction
    {
        public CarSensorReading CarSensorReading { get; set; }
        public CarSimulationTick CarResponse { get; set; }
    }

    class CarNeuralNetwork
    {
        private SequentialMinimalOptimization<Gaussian> teacher;
        private HashSet<(CarSensorReading)>

        public CarNeuralNetwork()
        {
            this.teacher = new SequentialMinimalOptimization<Gaussian>()
            {
                UseComplexityHeuristic = true,
                UseKernelEstimation = true
            };
        }

        public void Train(
            CarSensorReading sensorReading,
            CarSimulationTick carSimulationTick)
        {

        }

        public CarSimulationTick Ask(
            CarSensorReading sensorReading)
        {
            throw new NotImplementedException();
        }
    }
}
