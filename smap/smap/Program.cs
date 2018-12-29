using System;
using System.Drawing;
using System.Drawing.Imaging;
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
            Console.WriteLine("Hello World!");
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
            var bmpStream = new MemoryStream(bmp);
            File.WriteAllBytes("FirstBmp.bmp", bmp);
            
//            var document = new PdfDocument();
//            document.Info.Title = "Binary data backup";
//
//            var page = document.AddPage();
//            var gfx = XGraphics.FromPdfPage(page);
//            var font = new XFont("Consolas", 12, XFontStyle.Regular);
//            
//            gfx.DrawString("Hello world!", font, XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.Center);
//
//            var image = XImage.FromStream(bmpStream);
//            // A4 width: 595 pt ; height: 842 pt 
//            gfx.DrawImage(image, 0,0, 250, 250);
//            
//            document.Save("first.pdf");

            var barcodeReader = new BarcodeReader();
            
            var bitmap = Image.FromFile("FirstBmp.bmp") as Bitmap;
            if (bitmap == null) throw new NullReferenceException("bitmap should not be null");
            
            var bitmapSource = new RGBLuminanceSource(ImageHelper.ImageToByteArray(bitmap), bitmap.Width, bitmap.Height);
            var result = barcodeReader.Decode(bitmapSource);
            
            var readMetaData = new MetaData();
            readMetaData.Parse(Encoding.UTF8.GetBytes(result.Text));
        }
    }
}