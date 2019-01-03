using System;
using System.Drawing;
using System.Linq;
using smap.Helpers;
using ZXing;

namespace smap.Data.Input
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
            readMetaData.Parse(Decoder.DecodeBase64(result.Text));

            var maxY = 0;
            var minY = bitmap.Height;
            foreach (var resultResultPoint in result.ResultPoints)
            {
                if (resultResultPoint.Y > maxY) maxY = (int) resultResultPoint.Y;
                if (resultResultPoint.Y < minY) minY = (int) resultResultPoint.Y;
            }

            var points = result.ResultPoints.ToList();
            points.Sort((a, b) => a.X < b.X ? -1 : a.X > b.X ? 1 : a.Y < b.Y ? -1 : a.Y > b.Y ? 1 : 0);

            var upperLeftPoint = points[0];
            var lowerLeftPoint = points[1];

            if (upperLeftPoint.Y > lowerLeftPoint.Y)
            {
                var tmp = upperLeftPoint;
                upperLeftPoint = lowerLeftPoint;
                lowerLeftPoint = tmp;
            }

            return new QrReaderData
            {
                MetaData = readMetaData,
                QrCodeBottomPositionY = maxY + (maxY - minY) / 2,
                PageRotation = (Math.Abs(upperLeftPoint.X - lowerLeftPoint.X) < 0.001 ? 0 : upperLeftPoint.X < lowerLeftPoint.X ? -1 : 1) * Math.Tan(Math.Abs(upperLeftPoint.X - lowerLeftPoint.X) / Math.Abs(upperLeftPoint.Y - lowerLeftPoint.Y)) * (180/Math.PI)
            };
        } 
    }
}