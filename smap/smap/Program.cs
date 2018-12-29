using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using ZXing;
using ZXing.QrCode;

namespace smap
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var bmpStream = CreateQrCode();
            
            CreatePdf(bmpStream, GenerateRandomString(2784).Split(58));

            // ReadQrCode();
        }

        private static void CreatePdf(MemoryStream qrCode, IEnumerable<string> data)
        {
            var document = new PdfDocument();
            document.Info.Title = "Binary data backup";

            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("Consolas", 12, XFontStyle.Regular);

            // 57 x 48
            var rowIndex = 0;
            foreach (var rowData in data)
            {
                for (var i = 0; i < rowData.Length; i++)
                {
                    gfx.DrawString(rowData[i].ToString(), font, XBrushes.Black, 37 + i * 9, 30 + 57 + 14 + rowIndex * 15);                    
                }
                rowIndex++;
            }
            
            var image = XImage.FromStream(qrCode);
            // A4 width: 595 pt ; height: 842 pt 
            gfx.DrawImage(image, 30, 30, 57, 57);

            document.Save("first.pdf");
        }

        private static MemoryStream CreateQrCode()
        {
            var metaData = new MetaData
            {
                DataChunkSize = 63,
                ChunkId = 1,
                TotalDataSize = 1024,
                Checksum = new byte[20]
            };

            var barcodeWriter = new BarcodeWriterPixelData()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions()
            };

            var pixelData = barcodeWriter.Write(Encoding.UTF8.GetString(metaData.ToBytes()));

            var bmp = ImageHelper.PixelDataToBmp(pixelData, 10);
            return new MemoryStream(bmp);
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