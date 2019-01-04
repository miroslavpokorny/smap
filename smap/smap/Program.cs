﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using smap.Data.Input;
using smap.Data.Output;
using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Neural.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SmapCommon;
using SmapCommon.Extensions;

namespace smap
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var dataOutput = new DataOutput(File.ReadAllBytes("./assets/Database.kdbx"));
//            dataOutput.SaveDataAsPdf("Database.pdf");

            var qrReader = new QrReader();
            var qrData = qrReader.ReadQrCode("assets/Database01.jpg");

            var columnNameToIndex = new Dictionary<string, int> {["class"] = 0};
            for (var i = 1; i <= 1024; i++)
            {
                columnNameToIndex[$"pixel{i}"] = i;
            }
            
            using (var image = Image.Load("assets/Database01.jpg"))
            {
                image.Mutate(x =>
                    x.Crop(new SixLabors.Primitives.Rectangle(0, qrData.QrCodeBottomPositionY,
                            x.GetCurrentSize().Width - 1, x.GetCurrentSize().Height - qrData.QrCodeBottomPositionY - 1))
                        .Rotate((float) qrData.PageRotation).BackgroundColor(Rgba32.White));
                var contentArea = image.GetContentArea();
                image.Mutate(x => x.Crop(contentArea));
                
                using (var fileStream = new FileStream("onlyData.jpg", FileMode.Create))
                {
                    image.SaveAsJpeg(fileStream);                    
                }

                var rowsPerPage = (int) Math.Ceiling(qrData.MetaData.DataChunkSize / (double) DataOutput.MaxLettersPerRow);

                var letters = new List<CsvRow>((int) qrData.MetaData.DataChunkSize);
                var lettersFallback = new List<CsvRow>((int) qrData.MetaData.DataChunkSize);
                var nextY = 0;
                for (var y = 0; y < rowsPerPage; y++)
                {
                    var nextRowContentArea = image.GetNextRowContentArea(nextY);
                    nextY = nextRowContentArea.Y + nextRowContentArea.Height;

                    var rowImage = image.Clone(img => img.Crop(nextRowContentArea));

                    using (var fileStream = new FileStream($"../assets/temp/rows/row_{y:D2}.jpg", FileMode.Create))
                    {
                        rowImage.SaveAsJpeg(fileStream);
                    }

                    var currentRowLetters = y == rowsPerPage - 1
                        ? qrData.MetaData.DataChunkSize - DataOutput.MaxLettersPerRow * y : DataOutput.MaxLettersPerRow;
                    var nextX = 0;
                    for (var x = 0; x < currentRowLetters; x++)
                    {
                        var nextLetterContentArea = rowImage.GetNextLetterContentArea(nextX);
                        nextX = nextLetterContentArea.X + nextLetterContentArea.Width;
                        
                        var letterImage = rowImage.Clone(img =>
                            img.Crop(nextLetterContentArea));

                        using (var memoryStream = new MemoryStream())
                        {
                            letterImage.SaveAsBmp(memoryStream);
                            memoryStream.Seek(0, 0);
                            letters.Add(new CsvRow(columnNameToIndex, LoadImageHelper.FileAsData(memoryStream)));                            
                            lettersFallback.Add(new CsvRow(columnNameToIndex, LoadImageHelper.FileAsData(memoryStream, threshold: 90)));                            
                        }
                        using (var fileStream = new FileStream($"../assets/temp/{y:D4}_{x:D4}.jpg", FileMode.Create))
                        {
                            letterImage.SaveAsJpeg(fileStream);
                        }
                    }
                }


                var iterationCounter = 0; 
                foreach (var lettersCollection in new[] { letters, lettersFallback })
                {
                    var csvReader = DataOutput.GetCsvParser(lettersCollection);
                    
                    var targetName = "class";
        
                    var featureNames = csvReader.EnumerateRows(c => c != targetName).First().ColumnNameToIndex.Keys.ToArray();
        
                    var testObservations = csvReader.EnumerateRows(featureNames).ToF64Matrix();
                
                    testObservations.Map(p => p / 255);
                    var model = ClassificationNeuralNetModel.Load(() => new StreamReader("../NeuralNetLearner/network.xml"));
                    var predictions = model.Predict(testObservations);
                
                    var stringBuilder = new StringBuilder();
                
                    foreach (var prediction in predictions)
                    {
                        stringBuilder.Append(Alphabet.Base32Alphabet.ToString()[(int) prediction]);
                    }
                
                    File.WriteAllText($"decoded{iterationCounter}.txt", stringBuilder.ToString());

                    var isChecksumCorrect = qrData.MetaData.Checksum.ToList()
                        .SequenceEqual(dataOutput.ComputeSha1Checksum(stringBuilder.ToString()).ToList());

                    Console.WriteLine($"{iterationCounter} => {isChecksumCorrect}");

                    if (isChecksumCorrect)
                    {
                        File.WriteAllText("decoded.txt", stringBuilder.ToString());
                        break;
                    }

                    iterationCounter++;
                }
            }
        }
        
        
    }
}