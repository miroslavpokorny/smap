using System.IO;
using System.Text;
using ZXing.Rendering;

namespace smap
{
    public class PixelDataHelper
    {
        /// <summary>
        /// Returns BMP file bytes according to format specified at https://en.wikipedia.org/wiki/BMP_file_format 
        /// </summary>
        /// <param name="pixelData"></param>
        /// <returns></returns>
        public static byte[] PixelDataToBmp(PixelData pixelData)
        {
            var headerSize = 14;
            var dibHeaderSize = 40;
            var fileSize = headerSize + dibHeaderSize + pixelData.Width * pixelData.Height * 4;
            
            var dibHeader = new byte[dibHeaderSize];
            using (var binaryWriter = new BinaryWriter(new MemoryStream(dibHeader)))
            {
                binaryWriter.Write(dibHeaderSize); // size of header
                binaryWriter.Write(pixelData.Width); // width
                binaryWriter.Write(pixelData.Height); // height
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
            
            var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true))
            {
                binaryWriter.Write(header);
                binaryWriter.Write(dibHeader);
                var pixels = pixelData.Pixels;
                for (var i = 0; i < pixels.Length; i += 4)
                {
                    var b = pixels[i];
                    var g = pixels[i + 1];
                    var r = pixels[i + 2];
                    binaryWriter.Write(r);
                    binaryWriter.Write(g);
                    binaryWriter.Write(b);
                    binaryWriter.Write((byte)0);
                }
            }

            memoryStream.Position = 0;

            using (var binaryReader = new BinaryReader(memoryStream))
            {
                return binaryReader.ReadBytes(fileSize);
            }
        }
    }
}