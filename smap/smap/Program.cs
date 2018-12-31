using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

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
            var qrData = qrReader.ReadQrCode("assets/Database01.jpg");

            using (var image = SixLabors.ImageSharp.Image.Load("assets/Database01.jpg"))
            {
                image.Mutate(x => x.Crop(new SixLabors.Primitives.Rectangle(0, qrData.QrCodeBottomPositionY, x.GetCurrentSize().Width - 1, x.GetCurrentSize().Height - qrData.QrCodeBottomPositionY - 1)));
                var contentArea = image.GetContentArea();
                image.Mutate(x => x.Crop(contentArea));

                var letterWidth = image.Width / (double)DataOutput.MaxLettersPerRow;
                var letterHeight = image.Height / (double)DataOutput.MaxRowsPerPage;

                for (var y = 0; y < DataOutput.MaxRowsPerPage; y++)
                {
                    for (var x = 0; x < DataOutput.MaxLettersPerRow; x++)
                    {
                        var posX = x * letterWidth;
                        var posY = y * letterHeight;
                        var letterImage = image.Clone(img =>
                            img.Crop(new SixLabors.Primitives.Rectangle((int) posX, (int) posY, (int) letterWidth, (int) letterHeight)));

                        
//                        using (var fileStream = new FileStream($"../assets/temp/{y:D4}_{x:D4}.jpg", FileMode.Create))
//                        {
//                            letterImage.SaveAsJpeg(fileStream);
//                        }
                    }
                }

//                using (var fileStream = new FileStream("onlyData.jpg", FileMode.Create))
//                {
//                    image.SaveAsJpeg(fileStream);                    
//                }

            }

//            var model = ClassificationNeuralNetModel.Load(() => new StreamReader("../NeuralNetLearner/network.xml"));
//
//            var csvReader = new CsvParser(() =>
//            {
//                var columnNameToIndex = new Dictionary<string, int> {["class"] = 0};
//                for (var i = 1; i <= 1024; i++)
//                {
//                    columnNameToIndex[$"pixel{i}"] = i;
//                }
//
//                var memoryStream = new MemoryStream();
//                var csvWriter = new CsvWriter(() => new StreamWriter(memoryStream, Encoding.Default, 4096, true));
//                csvWriter.Write(new[] {"3.jpg", "D.jpg", "N.jpg", "U.jpg", "V.jpg"}.Select(fileName =>
//                    new CsvRow(columnNameToIndex, FileAsData(fileName))));
//                memoryStream.Seek(0, 0);
//                return new StreamReader(memoryStream);
//            });
//            
//            var targetName = "class";
//
//            var featureNames = csvReader.EnumerateRows(c => c != targetName).First().ColumnNameToIndex.Keys.ToArray();
//
//            var testObservations = csvReader.EnumerateRows(featureNames).ToF64Matrix();
//            var testTargets = csvReader.EnumerateRows(targetName).ToF64Vector();
//            
//            testObservations.Map(p => p / 255);
//
//            var predictions = model.Predict(testObservations);

        }
        
        private static string[] FileAsData(string fileName)
        {
            var result = new string[1025];
            result[0] = Alphabet.Base32Alphabet.ToString().IndexOf(Path.GetFileName(fileName)[0]).ToString();
            using (var bitmap = System.Drawing.Image.FromFile(fileName) as Bitmap)
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
                                foreach (var pixel in row)
                                {
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