using System;
using System.IO;

namespace NeuralNetLearner
{
    class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            Directory.CreateDirectory("assets");
            var dataGenerator = new DataGenerator();
            Console.WriteLine("Generating learning data");            
            dataGenerator.GenerateLearningData();
            var time1 = stopwatch.Elapsed;
            Console.WriteLine($"Done in {time1}");
            Console.WriteLine("Generating csv file");
            dataGenerator.GenerateCsv();
            var time2 = stopwatch.Elapsed;
            Console.WriteLine($"Done in {time2 - time1}");
            var networkHelper = new NetworkHelper("data.csv");
            Console.WriteLine("Learning network");
            networkHelper.LearnNetwork().Save(() => new StreamWriter("network.xml"));
            var time3 = stopwatch.Elapsed;
            Console.WriteLine($"Done in {time3 - time2}");
            Console.WriteLine($"Total time of execution {stopwatch.Elapsed}");
        }
    }
}