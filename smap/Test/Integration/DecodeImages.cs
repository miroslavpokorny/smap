using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Test.Integration
{
    [TestFixture]
    public class DecodeImages : IntegrationBase
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
#if DEBUG
            File.Copy($"{ToProjectDir}../NeuralNetLearner/network.xml", $"{ToProjectDir}/bin/Debug/netcoreapp2.1/network.xml", true);
#else
            File.Copy($"{ToProjectDir}../NeuralNetLearner/network.xml", $"{ToProjectDir}/bin/Release/netcoreapp2.1/network.xml", true);
#endif
        }
        
        [Test]
        [TestCase("randomFile01", "dat")]
        [TestCase("randomFile02", "dat")]
        [TestCase("randomFile03", "dat")]
        [TestCase("randomFile04", "dat")]
        [TestCase("randomFile05", "dat")]
        [TestCase("Database01", "kdbx")]
        [TestCase("Database02", "kdbx")]
        public void ShouldDecodeImageFileAndOutputSameFile(string fileName, string fileExtension)
        {
            var filesToDecode = Directory.GetFiles($"{ToProjectDir}Resources/Approve/Images").Where(x => x.Contains(fileName));
            PrepareDirInTempDir(fileName);
            foreach (var fileToDecode in filesToDecode)
            {
                CopyFileToTempDirectory(fileName, fileToDecode);
            }

            RunProcessAsync("dotnet", $" {SmapProgram} -i {ToProjectDir}Resources/temp/{fileName} -o {ToProjectDir}Resources/temp/{fileName}/output -m Decode").Wait();

            Assert.True(File.Exists($"{ToProjectDir}Resources/temp/{fileName}/output/decoded"));
            Assert.True(File.ReadAllBytes($"{ToProjectDir}Resources/Approve/{fileName}.{fileExtension}").ToList().SequenceEqual(File.ReadAllBytes($"{ToProjectDir}Resources/temp/{fileName}/output/decoded")));
        }

        private void PrepareDirInTempDir(string directoryName)
        {
            var directoryPath = $"{ToProjectDir}Resources/temp/{directoryName}";
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);
            Directory.CreateDirectory($"{directoryPath}/output");
        }

        private void CopyFileToTempDirectory(string directoryName, string fileName)
        {
            var directoryPath = $"{ToProjectDir}Resources/temp/{directoryName}";
            File.Copy(fileName, $"{directoryPath}/{Path.GetFileName(fileName)}");
            
        }
    }
}