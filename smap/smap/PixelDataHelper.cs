using System;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
using ZXing.Rendering;

namespace smap
{
    public static class PixelDataHelper
    {
        /// <summary>
        /// Returns BMP file bytes according to format specified at https://en.wikipedia.org/wiki/BMP_file_format 
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="enlargeMultiplier"></param>
        /// <returns></returns>
        public static byte[] PixelDataToBmp(PixelData pixelData, uint enlargeMultiplier = 1)
        {
            var headerSize = 14;
            var dibHeaderSize = 40;
            var enlargedWidth = (int)(pixelData.Width * enlargeMultiplier);
            var enlargedHeight = (int)(pixelData.Height * enlargeMultiplier);
            var fileSize = headerSize + dibHeaderSize + enlargedWidth * enlargedHeight * 4;
            
            var dibHeader = new byte[dibHeaderSize];
            
            using (var binaryWriter = new BinaryWriter(new MemoryStream(dibHeader)))
            {
                binaryWriter.Write(dibHeaderSize); // size of header
                binaryWriter.Write(enlargedWidth); // width
                binaryWriter.Write(enlargedHeight); // height
                binaryWriter.Write((ushort) 1); // number of color planes
                binaryWriter.Write((ushort) 32); // bits per pixel
                binaryWriter.Write(0); // compression method
                binaryWriter.Write(0); // image size
                binaryWriter.Write(0); // horizontal resolution
                binaryWriter.Write(0); // vertical resolution
                binaryWriter.Write(0); // number of colors in color palate
                binaryWriter.Write(0); // number of important colors used
            }
            
            var header = new byte[headerSize];
            using (var binaryWriter = new BinaryWriter(new MemoryStream(header)))
            {
                binaryWriter.Write(new byte[] {0x42, 0x4D});
                binaryWriter.Write(fileSize);
                binaryWriter.Write((uint) 0);
                binaryWriter.Write(headerSize + dibHeaderSize);
            }
            
            var image = new Image(pixelData.Width, pixelData.Height, enlargeMultiplier);
            var pixels = pixelData.Pixels;
            for (var i = 0; i < pixels.Length; i += 4)
            {
                var x = i / 4 % pixelData.Width;
                var y = i / 4 / pixelData.Width;
                
                var pixel = image.GetPixelAt(x, y);
                pixel.B = pixels[i];
                pixel.G = pixels[i + 1];
                pixel.R = pixels[i + 2];
            }
            var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true))
            {
                binaryWriter.Write(header);
                binaryWriter.Write(dibHeader);
                for (var y = 0; y < enlargedHeight; y++)
                {
                    for (var x = 0; x < enlargedWidth; x++)
                    {
                        var pixel = image.GetEnlargedPixelAt(x, y);
                        binaryWriter.Write(pixel.R);
                        binaryWriter.Write(pixel.G);
                        binaryWriter.Write(pixel.B);
                        binaryWriter.Write((byte)0);
                    }
                }
            }

            memoryStream.Position = 0;

            using (var binaryReader = new BinaryReader(memoryStream))
            {
                return binaryReader.ReadBytes(fileSize);
            }
        }

        private class Image
        {
            private readonly Pixel[][] _pixels;
            private readonly uint _enlargeMultiplier;

            public Image(int width, int height, uint enlargeMultiplier)
            {
                _enlargeMultiplier = enlargeMultiplier;
                _pixels = new Pixel[width][];
                for (var x = 0; x < width; x++)
                {
                    _pixels[x] = new Pixel[height];
                    for (int y = 0; y < height; y++)
                    {
                        _pixels[x][y] = new Pixel();
                    }
                }
            }

            public Pixel GetPixelAt(int x, int y)
            {
                return _pixels[x][y];
            }

            public Pixel GetEnlargedPixelAt(int x, int y)
            {
                // TODO write this method correctly
                return _pixels[x / _enlargeMultiplier][y / _enlargeMultiplier];
            }
        }

        private class Pixel
        {
            public byte R { get; set; }
            public byte G { get; set; }
            public byte B { get; set; }
        }
    }
}