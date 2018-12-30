using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace NeuralNetLearner
{
    public class DataGenerator
    {
        private readonly string _alphabet = smap.Alphabet.Base32Alphabet.ToString();
        private char _letter;
        
        public void GenerateLearningData()
        {
            foreach (var letter in _alphabet)
            {
                _letter = letter;
                GenerateBaseLetters();
            }
            GenerateBlurLetters();
            GenerateRotatedLetters();
        }

        private void GenerateBaseLetters()
        {
            var counter = 0;
            foreach (var pointF in new Offset(-6, 7, -10, -6).GetOffsets())
            {
                var bitmap = new Bitmap(32, 32);
                using (var graphics = Graphics.FromImage(bitmap))
                {                
                    graphics.FillRectangle(Brushes.White, 0, 0, 32, 32);                       
                    using (var font = new Font("Consolas", 26))
                    {
                        graphics.DrawString(_letter.ToString(), font, Brushes.Black, pointF);
                    }
                }
                
                bitmap.Save($"assets/{_letter}_{++counter:D3}.bmp", ImageFormat.Bmp);
            }
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
        
        
    }
}