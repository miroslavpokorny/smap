using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharpLearning.InputOutput.Csv;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SmapCommon;
using SmapCommon.Extensions;
using Image = SixLabors.ImageSharp.Image;

namespace NeuralNetLearner
{
    public class DataGenerator
    {
        private readonly string _alphabet = Alphabet.Base32Alphabet.ToString();
        private char _letter;
        
        public void GenerateLearningData()
        {
            foreach (var letter in _alphabet)
            {
                _letter = letter;
                GenerateBaseLetters();
            }
            // GenerateBlurLetters();
            // GenerateRotatedLetters();
        }

        private class GenerateLetterData
        {
            public int FontSize { get; set; }
            public int OffsetX { get; set; }
            public int OffsetY { get; set; }
        }

        private void GenerateBaseLetters()
        {
            var counter = 0;
            foreach (var data in new GenerateLetterData[]
            {
                new GenerateLetterData { FontSize = 26, OffsetX = 0, OffsetY = -8 },
                new GenerateLetterData { FontSize = 20, OffsetX = 3, OffsetY = -5 },
                new GenerateLetterData { FontSize = 18, OffsetX = 6, OffsetY = 0 },
                new GenerateLetterData { FontSize = 14, OffsetX = 8, OffsetY = 0 },
                new GenerateLetterData { FontSize = 12, OffsetX = 10, OffsetY = 0 }
            })
            {
                var bitmap = new Bitmap(32, 32);
                using (var graphics = Graphics.FromImage(bitmap))
                {                
                    graphics.FillRectangle(Brushes.White, 0, 0, 32, 32);                       
                    using (var font = new Font("Consolas", data.FontSize))
                    {
                        graphics.DrawString(_letter.ToString(), font, Brushes.Black, data.OffsetX, data.OffsetY);
                    }
                
                    bitmap.Save($"assets/{_letter}_{++counter:D3}.bmp", ImageFormat.Bmp);
                }                
            }
            
//            foreach (var pointF in new Offset(-6, 7, -10, -6).GetOffsets())
//            {
//                var bitmap = new Bitmap(32, 32);
//                using (var graphics = Graphics.FromImage(bitmap))
//                {                
//                    graphics.FillRectangle(Brushes.White, 0, 0, 32, 32);                       
//                    using (var font = new Font("Consolas", 26))
//                    {
//                        graphics.DrawString(_letter.ToString(), font, Brushes.Black, pointF);
//                    }
//                }
//                
//                bitmap.Save($"assets/{_letter}_{++counter:D3}.bmp", ImageFormat.Bmp);
//            }
        }

        private void GenerateBlurLetters()
        {
            foreach (var file in Directory.GetFiles("assets"))
            {
                var counter = 0;
                foreach (var blur in new [] {0.7f, 1, 1.2f, 1.5f})
                {
                    using (var image = Image.Load(file))
                    {
                        image.Mutate(x => x.GaussianBlur(blur).Grayscale());
                        using (var fileStream =
                            new FileStream($"assets/{Path.GetFileNameWithoutExtension(file)}_B_{++counter:D3}.bmp",
                                FileMode.Create))
                        {
                            image.SaveAsBmp(fileStream);
                        }
                    }
                }
            }
        }

        private void GenerateRotatedLetters()
        {
            foreach (var file in Directory.GetFiles("assets"))
            {
                var counter = 0;
                foreach (var i in Enumerable.Range(-4, 9))
                {
                    using (var image = Image.Load(file))
                    {
                        image.Mutate(x => x.BackgroundColor(Rgba32.White).Rotate(i / 0.5f).Grayscale().BackgroundColor(Rgba32.White).Resize(32, 32));
                        using (var fileStream =
                            new FileStream($"assets/{Path.GetFileNameWithoutExtension(file)}_R_{++counter:D3}.bmp",
                                FileMode.Create))
                        {
                            image.SaveAsBmp(fileStream);
                        }
                    }
                }
            }
        }


        private class Offset
        {
            private readonly int _minX;
            private readonly int _maxX;
            private readonly int _minY;
            private readonly int _maxY;

            public Offset(int minX, int maxX, int minY, int maxY)
            {
                _minX = minX;
                _maxX = maxX;
                _minY = minY;
                _maxY = maxY;
            }

            public IEnumerable<PointF> GetOffsets()
            {
                return Enumerable.Range(_minX, _maxX - _minX).SelectMany(x =>
                    Enumerable.Range(_minY, _maxY - _minY).Select(y => new PointF(x, y)));
            }
        }


        public void GenerateCsv()
        {
            var columnNameToIndex = new Dictionary<string, int>();
            columnNameToIndex["class"] = 0;
            for (var i = 1; i <= 1024; i++)
            {
                columnNameToIndex[$"pixel{i}"] = i;
            }
            var writer = new CsvWriter(() => new StreamWriter(new FileStream("data.csv", FileMode.Create)));
            
            writer.Write(Directory.GetFiles("assets").Select(fileName => new CsvRow(columnNameToIndex, LoadImageHelper.FileAsData(fileName))));
        }

        
    }
}