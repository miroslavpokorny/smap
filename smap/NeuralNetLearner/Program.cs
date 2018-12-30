using System.IO;

namespace NeuralNetLearner
{
    class Program
    {
        static void Main(string[] args)
        {
            Directory.CreateDirectory("assets");
            var dataGenerator = new DataGenerator();
            dataGenerator.GenerateLearningData();
        }
    }
}