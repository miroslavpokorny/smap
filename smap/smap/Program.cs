using System;
using System.Drawing;
using System.IO;
using System.Text;
using ZXing;

namespace smap
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var dataOutput = new DataOutput(File.ReadAllBytes("./assets/Database.kdbx"));
            dataOutput.SaveDataAsPdf("Database.pdf");

            // ReadQrCode();
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