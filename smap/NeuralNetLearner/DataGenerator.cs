using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SharpLearning.InputOutput.Csv;
using SmapCommon;

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

            // Added generation of pad character
            _letter = '-';
            GenerateBaseLetters();
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
            foreach (var data in new[]
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