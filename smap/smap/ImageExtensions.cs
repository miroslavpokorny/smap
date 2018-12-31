using System.Drawing;

namespace smap
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
    }
}