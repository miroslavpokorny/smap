using System;
using System.IO;
using System.Text;
using ZXing;
using ZXing.QrCode;

namespace smap
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var metaData = new MetaData
            {
                DataChunkSize = 128,
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

            var bmp = PixelDataHelper.PixelDataToBmp(pixelData);
            File.WriteAllBytes("FirstBmp.bmp", bmp);
        }
    }
}