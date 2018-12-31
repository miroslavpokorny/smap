using System;
using System.Drawing;
using System.Numerics;
using System.Text;
using ZXing;

namespace smap
{
    public class QrReader
    {
        public QrReaderData ReadQrCode(string filePath)
        {
            var barcodeReader = new BarcodeReader();
            
            var bitmap = Image.FromFile(filePath) as Bitmap;
            if (bitmap == null) throw new NullReferenceException("bitmap should not be null");
            
            var bitmapSource = new RGBLuminanceSource(ImageHelper.ImageToByteArray(bitmap), bitmap.Width, bitmap.Height);
            var result = barcodeReader.Decode(bitmapSource);
            
            var readMetaData = new MetaData();
            readMetaData.Parse(Encoding.UTF8.GetBytes(result.Text));

            var maxY = 0;
            var minY = bitmap.Height;
            foreach (var resultResultPoint in result.ResultPoints)
            {
                if (resultResultPoint.Y > maxY) maxY = (int) resultResultPoint.Y;
                if (resultResultPoint.Y < minY) minY = (int) resultResultPoint.Y;
            }

            return new QrReaderData
            {
                MetaData = readMetaData,
                QrCodeBottomPositionY = maxY + (maxY - minY) / 5
            };
        } 
    }
}