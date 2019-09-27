﻿using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Statistics.Kernels;
using Accord.Statistics.Models.Regression.Linear;
using MachineLearningPractice.Models;
using System;
using System.Collections.Generic;
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
        private MultivariateLinearRegression regression;

        private readonly OrdinaryLeastSquares teacher;

        private readonly IList<CarSimulationTick> trainingInstructions;

        public CarNeuralNetwork()
        {
            this.trainingInstructions = new List<CarSimulationTick>();
            this.teacher = new OrdinaryLeastSquares();
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

        public void Train()
        {
            var inputs = this.trainingInstructions
                .Select(x => x.CarSensorReading)
                .Select(x => GetNeuralInputFromSensorReading(x))
                .ToArray();

            var outputs = trainingInstructions
                .Select(x => x.CarResponse)
                .Select(x => new[]
                {
                    x.AccelerationDeltaVelocity,
                    x.TurnDeltaAngle
                })
                .ToArray();

            this.regression = teacher.Learn(inputs, outputs);
        }

        public CarResponse Ask(
            CarSensorReadingSnapshot sensorReading)
        {
            if(this.regression == null)
                return new CarResponse();

            var prediction = this.regression.Transform(
                GetNeuralInputFromSensorReading(sensorReading));
            return new CarResponse()
            {
                AccelerationDeltaVelocity = prediction[0],
                TurnDeltaAngle = prediction[1]
            };
        }

        private static double[] GetNeuralInputFromSensorReading(CarSensorReadingSnapshot sensorReading)
        {
            return new[] {
                sensorReading.CenterSensor.Distance,
                sensorReading.LeftSensor.Distance,
                sensorReading.RightSensor.Distance
            };
        }
    }
}
