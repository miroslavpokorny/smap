using System.IO;
using System.Linq;
using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Neural;
using SharpLearning.Neural.Activations;
using SharpLearning.Neural.Layers;
using SharpLearning.Neural.Learners;
using SharpLearning.Neural.Loss;
using SharpLearning.Neural.Models;

namespace NeuralNetLearner
{
    public class NetworkHelper
    {
        private readonly double[] _trainingTargets;
        private readonly F64Matrix _trainingObservations;
        private const string TargetName = "class";

        private int NumberOfClasses => _trainingTargets.Distinct().Count();
        
        public NetworkHelper(string inputFileName)
        {
            var trainingParser = new CsvParser(() => new StreamReader(new FileStream(inputFileName, FileMode.Open)));

            var featureNames = trainingParser.EnumerateRows(c => c != TargetName).First().ColumnNameToIndex.Keys
                .ToArray();

            _trainingObservations = trainingParser.EnumerateRows(featureNames).ToF64Matrix();
            _trainingTargets = trainingParser.EnumerateRows(TargetName).ToF64Vector();
            
            _trainingObservations.Map(p => p / 255);
        }
        
        private NeuralNet GetNeuralNetwork()
        {
            var neuralNet = new NeuralNet();
            neuralNet.Add(new InputLayer(32, 32, 1));
            neuralNet.Add(new Conv2DLayer(5, 5, 32));
            neuralNet.Add(new MaxPool2DLayer(2, 2));
            neuralNet.Add(new DropoutLayer(0.5));
            neuralNet.Add(new DenseLayer(256, Activation.Relu));
            neuralNet.Add(new DropoutLayer(0.5));
            neuralNet.Add(new SoftMaxLayer(NumberOfClasses));
            return neuralNet;
        }

        public ClassificationNeuralNetModel LearnNetwork()
        {
            var learner = new ClassificationNeuralNetLearner(GetNeuralNetwork(), iterations: 10, loss: new AccuracyLoss());
            return learner.Learn(_trainingObservations, _trainingTargets);
        }
    }
}