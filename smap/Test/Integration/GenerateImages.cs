using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageMagick;
using NUnit.Framework;

namespace Test.Integration
{
    [TestFixture]
    public class GenerateImages : IntegrationBase
    {

        // Generate new PDFs files
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            var fileNames = new List<string>()
            {
                "randomFile01.dat",
                "randomFile02.dat",
                "randomFile03.dat",
                "randomFile04.dat",
                "randomFile05.dat"
            };
            
            var tasks = new List<Task>();
            foreach (var fileName in fileNames)
            {
                tasks.Add(RunProcessAsync("dotnet",
                    $" {SmapProgram} -i {ToProjectDir}Resources/Approve/{fileName} -o {ToProjectDir}Resources/temp/{Path.GetFileNameWithoutExtension(fileName)}.pdf -m Encode"));                
            }

            Task.WhenAll(tasks).Wait();
        }
        
        
        // To ensure correctly execution of this test you should have installed ImageMagick and GhostScript at your system =>
        // https://www.imagemagick.org/script/download.php
        // https://www.ghostscript.com/download/gsdnld.html
        [Test]
        [TestCase("Resources/temp/randomFile01.pdf")]
        [TestCase("Resources/temp/randomFile02.pdf")]
        [TestCase("Resources/temp/randomFile03.pdf")]
        [TestCase("Resources/temp/randomFile04.pdf")]
        [TestCase("Resources/temp/randomFile05.pdf")]
        public void GenerateAndCheckImageFiles(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var settings = new MagickReadSettings {Density = new Density(300, 300)};
            
            if (!Directory.Exists($"{ToProjectDir}Resources/temp/"))
            {
                Directory.CreateDirectory($"{ToProjectDir}Resources/temp/");
            }

            var fileNames = new List<string>();
            
            using (var images = new MagickImageCollection())
            {
                images.Read($"{ToProjectDir}{filePath}", settings);
                var page = 1;
                foreach (var image in images)
                {
                    var currentFileName = $"{fileName}-{page:D2}.jpg";
                    fileNames.Add(currentFileName);
                    image.Format = MagickFormat.Jpg;
                    image.Write($"{ToProjectDir}Resources/temp/{currentFileName}");    
                    page++;
                }
            }

            foreach (var newFileName in fileNames)
            {
                var approveBytes = File.ReadAllBytes($"{ToProjectDir}Resources/Approve/Images/{newFileName}");
                var newBytes = File.ReadAllBytes($"{ToProjectDir}Resources/temp/{newFileName}");
                Assert.True(approveBytes.ToList().SequenceEqual(newBytes));
            }

            

        }
    }
}