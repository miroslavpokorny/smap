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
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using ZXing;

namespace smap
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
//            var dataOutput = new DataOutput(File.ReadAllBytes("./assets/Database.kdbx"));
//            dataOutput.SaveDataAsPdf("Database.pdf");

            // ReadQrCode();

            var model = ClassificationNeuralNetModel.Load(() => new StreamReader("../NeuralNetLearner/network.xml"));

            var csvReader = new CsvParser(() =>
            {
                var columnNameToIndex = new Dictionary<string, int> {["class"] = 0};
                for (var i = 1; i <= 1024; i++)
                {
                    columnNameToIndex[$"pixel{i}"] = i;
                }

                var memoryStream = new MemoryStream();
                var csvWriter = new CsvWriter(() => new StreamWriter(memoryStream, Encoding.Default, 4096, true));
                csvWriter.Write(new[] {"3.jpg", "D.jpg", "N.jpg", "U.jpg", "V.jpg"}.Select(fileName =>
                    new CsvRow(columnNameToIndex, FileAsData(fileName))));
                memoryStream.Seek(0, 0);
                return new StreamReader(memoryStream);
            });
            
            var targetName = "class";

            var featureNames = csvReader.EnumerateRows(c => c != targetName).First().ColumnNameToIndex.Keys.ToArray();

            var testObservations = csvReader.EnumerateRows(featureNames).ToF64Matrix();
            var testTargets = csvReader.EnumerateRows(targetName).ToF64Vector();
            
            testObservations.Map(p => p / 255);

            var predictions = model.Predict(testObservations);

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

        private static void ReadQrCode()
        {
            var barcodeReader = new BarcodeReader();
            
            var bitmap = Image.FromFile("FirstBmp.bmp") as Bitmap;
            if (bitmap == null) throw new NullReferenceException("bitmap should not be null");
            
            var bitmapSource = new RGBLuminanceSource(ImageHelper.ImageToByteArray(bitmap), bitmap.Width, bitmap.Height);
            var result = barcodeReader.Decode(bitmapSource);
            
            var readMetaData = new MetaData();
            readMetaData.Parse(Encoding.UTF8.GetBytes(result.Text));
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