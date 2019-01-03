using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;

namespace SmapCommon.Extensions
{
    public static class ImageExtensions
    {
        public static void Threshold(this Bitmap bitmap, int threshold)
        {
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    bitmap.SetPixel(x, y, (pixel.R + pixel.G + pixel.B) / 3 > threshold ? Color.White : Color.Black);
                }
            }
        }
        
        public static SixLabors.Primitives.Rectangle GetContentArea(this Bitmap bitmap)
        {
            var minX = bitmap.Width;
            var maxX = 0;
            var minY = bitmap.Height;
            var maxY = 0;
            
            for (var x = 0; x < bitmap.Width; x++)
            {
                for (var y = 0; y < bitmap.Height; y++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    var color = (pixel.R + pixel.G + pixel.B) / 3;
                    if (color != 0) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            
            return new SixLabors.Primitives.Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }
        
        public static SixLabors.Primitives.Rectangle GetContentArea(this Image<Rgba32> image)
        {
            var minX = image.Width;
            var maxX = 0;
            var minY = image.Height;
            var maxY = 0;

            for (var y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    var color = (pixel.R + pixel.G + pixel.B) / 3;
                    if (color > 128) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            
            return new SixLabors.Primitives.Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static SixLabors.Primitives.Rectangle GetNextLetterContentArea(this Image<Rgba32> image, int offsetX)
        {
            var minX = offsetX;
            for (var x = minX; x < image.Width; x++)
            {
                if (!IsVerticalLineWhite(image, x))
                {
                    break;
                }
                minX = x;
            }

            var maxX = minX + 1;
            for (var x = maxX; x < image.Width; x++)
            {
                if (IsVerticalLineWhite(image, x))
                {
                    break;
                }
                maxX = x;
            }
            
            var minY = image.Height;
            var maxY = 0;

            for (var y = 0; y < image.Height; y++)
            {
                var row = image.GetPixelRowSpan(y);
                for (var x = minX; x <= maxX; x++)
                {
                    var pixel = row[x];
                    var color = (pixel.R + pixel.G + pixel.B) / 3;
                    if (color > 128) continue;
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                }
            }
            
            return new SixLabors.Primitives.Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static SixLabors.Primitives.Rectangle GetNextRowContentArea(this Image<Rgba32> image, int offsetY)
        {
            var minY = offsetY;
            for (var y = minY; y < image.Height; y++)
            {
                if (!IsHorizontalLineWhite(image, y))
                {
                    break;
                }

                minY = y;
            }

            var maxY = minY + 1;
            for (var y = maxY; y < image.Height; y++)
            {
                if (IsHorizontalLineWhite(image, y))
                {
                    break;
                }

                maxY = y;
            }

            var minX = image.Width;
            var maxX = 0;
            
            for (var y = minY; y <= maxY; y++)
            {
                var row = image.GetPixelRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    var pixel = row[x];
                    var color = (pixel.R + pixel.G + pixel.B) / 3;
                    if (color > 128) continue;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
            
            return new SixLabors.Primitives.Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        private static bool IsVerticalLineWhite(Image<Rgba32> image, int x)
        {
            var pixels = image.GetPixelSpan();
            for (var y = 0; y < image.Height; y++)
            {
                var index = x + y * image.Width;
                var pixel = pixels[index];
                var color = (pixel.R + pixel.G + pixel.B) / 3;
                if (color < 128)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsHorizontalLineWhite(Image<Rgba32> image, int y)
        {
            var row = image.GetPixelRowSpan(y);
            foreach (var pixel in row)
            {
                var color = (pixel.R + pixel.G + pixel.B) / 3;
                if (color < 128)
                {
                    return false;
                }
            }

            return true;
        }
    }
}