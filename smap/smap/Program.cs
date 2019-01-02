using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Neural.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace smap
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//            var dataOutput = new DataOutput(File.ReadAllBytes("./assets/Database.kdbx"));
//            dataOutput.SaveDataAsPdf("Database.pdf");

            var qrReader = new QrReader();
            var qrData = qrReader.ReadQrCode("assets/Database-page-001.jpg");
            // var qrData = qrReader.ReadQrCode("assets/Database01.jpg");

            var columnNameToIndex = new Dictionary<string, int> {["class"] = 0};
            for (var i = 1; i <= 1024; i++)
            {
                columnNameToIndex[$"pixel{i}"] = i;
            }
            
            using (var image = SixLabors.ImageSharp.Image.Load("assets/Database01.jpg"))
            {
                image.Mutate(x =>
                    x.Crop(new SixLabors.Primitives.Rectangle(0, qrData.QrCodeBottomPositionY,
                            x.GetCurrentSize().Width - 1, x.GetCurrentSize().Height - qrData.QrCodeBottomPositionY - 1))
                        .Rotate((float) qrData.PageRotation).BackgroundColor(Rgba32.White));
                var contentArea = image.GetContentArea();
                image.Mutate(x => x.Crop(contentArea));

                // TODO replace constants with data from metaData
                // var letterWidth = image.Width / (double)DataOutput.MaxLettersPerRow;
                var letterHeight = image.Height / (double)DataOutput.MaxRowsPerPage;

                var letters = new List<CsvRow>(DataOutput.MaxLettersPerPage);
                for (var y = 0; y < DataOutput.MaxRowsPerPage; y++)
                {
                    var posY = y * letterHeight;
                    var rowImage = image.Clone(img =>
                        img.Crop(new SixLabors.Primitives.Rectangle(0, (int) posY, img.GetCurrentSize().Width,
                            (int) letterHeight)));
                    var rowArea = rowImage.GetContentArea();
                    var margin = (int) (rowArea.Width / (double) DataOutput.MaxRowsPerPage / 3);
                    rowArea.Width += margin;
                    rowArea.X = rowArea.X - margin;
                    rowImage.Mutate(x => x.Crop(rowArea).BackgroundColor(Rgba32.White));


                    var nextX = 0;
                    for (var x = 0; x < DataOutput.MaxLettersPerRow; x++)
                    {

                        var nextLetterContentArea = rowImage.GetNextLetterContentArea(nextX);
                        nextX = nextLetterContentArea.X + nextLetterContentArea.Width;
                        
                        var letterImage = rowImage.Clone(img =>
                            img.Crop(nextLetterContentArea));

                        using (var memoryStream = new MemoryStream())
                        {
                            letterImage.SaveAsBmp(memoryStream);
                            memoryStream.Seek(0, 0);
                            letters.Add(new CsvRow(columnNameToIndex, FileAsData(memoryStream)));                            
                        }
                        using (var fileStream = new FileStream($"../assets/temp/{y:D4}_{x:D4}.jpg", FileMode.Create))
                        {
                            letterImage.SaveAsJpeg(fileStream);
                        }
                    }
                }
//                using (var fileStream = new FileStream("onlyData.jpg", FileMode.Create))
//                {
//                    image.SaveAsJpeg(fileStream);                    
//                }

                var csvReader = new CsvParser(() =>
                {
                    var memoryStream = new MemoryStream();
                    var csvWriter = new CsvWriter(() => new StreamWriter(memoryStream, Encoding.Default, 4096, true));
                    csvWriter.Write(letters);
                    memoryStream.Seek(0, 0);
                    return new StreamReader(memoryStream);
                });
                
                var targetName = "class";
        
                var featureNames = csvReader.EnumerateRows(c => c != targetName).First().ColumnNameToIndex.Keys.ToArray();
        
                var testObservations = csvReader.EnumerateRows(featureNames).ToF64Matrix();
                // var testTargets = csvReader.EnumerateRows(targetName).ToF64Vector();
                
                testObservations.Map(p => p / 255);
                var model = ClassificationNeuralNetModel.Load(() => new StreamReader("../NeuralNetLearner/network.xml"));
                var predictions = model.Predict(testObservations);
                
                var stringBuilder = new StringBuilder();
                
                foreach (var prediction in predictions)
                {
                    stringBuilder.Append(Alphabet.Base32Alphabet.ToString()[(int) prediction]);
                }
                
                File.WriteAllText("decoded.txt", stringBuilder.ToString());
            }
        }
        
        private static string[] FileAsData(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                return FileAsData(fileStream,
                    Alphabet.Base32Alphabet.ToString().IndexOf(Path.GetFileName(fileName)[0]));
            }
        }

        private static string[] FileAsData(Stream stream, int expectedResult = -1)
        {
            var result = new string[1025];
            result[0] = expectedResult.ToString();
            using (var bitmap = System.Drawing.Image.FromStream(stream) as Bitmap)
            {
                if (bitmap == null) throw new NullReferenceException("bitmap should not be null");
                bitmap.Threshold(128);
                var contentArea = bitmap.GetContentArea();
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Bmp);
                    memoryStream.Seek(0, 0);
                    using (var image = SixLabors.ImageSharp.Image.Load(memoryStream))
                    {
                        image.Mutate(x => x.Crop(contentArea).Resize(32, 32));
                        using (var modifiedImageMemoryStream = new MemoryStream())
                        {
                            SixLabors.ImageSharp.ImageExtensions.SaveAsBmp(image, modifiedImageMemoryStream);
                            var index = 1;
                            for (var y = 0; y < 32; y++)
                            {
                                var row = image.GetPixelRowSpan(y);
                                for (var x = 0; x < 32; x++)
                                {
                                    var pixel = row[x];
                                    
                                    result[index++] = ((pixel.R + pixel.G + pixel.B) / 3).ToString();
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        private static string GenerateRandomString(int strLen)
        {
            var alphabet = Alphabet.Base32Alphabet.ToString();
            var stringBuilder = new StringBuilder(strLen);
            var random = new Random();
            for (var i = 0; i < strLen; i++)
            {
                stringBuilder.Append(alphabet[random.Next(0, alphabet.Length)]);
            }

            return stringBuilder.ToString();
        }
    }
}