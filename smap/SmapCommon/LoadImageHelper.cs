using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SmapCommon.Extensions;

namespace SmapCommon
{
    public static class LoadImageHelper
    {
        public static string[] FileAsData(string fileName)
        {
            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                return FileAsData(fileStream,
                    Alphabet.Base32Alphabet.ToString().IndexOf(Path.GetFileName(fileName)[0]));
            }
        }
        
        public static string[] FileAsData(Stream stream, int expectedResult = -1, byte threshold = 128)
        {
            var result = new string[1025];
            result[0] = expectedResult.ToString();
            using (var bitmap = System.Drawing.Image.FromStream(stream) as Bitmap)
            {
                if (bitmap == null) throw new NullReferenceException("bitmap should not be null");
                bitmap.Threshold(threshold);
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
                            image.SaveAsBmp(modifiedImageMemoryStream);
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
    }
}