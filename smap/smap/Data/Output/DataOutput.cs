using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using smap.Helpers;
using SharpLearning.InputOutput.Csv;
using SmapCommon.Extensions;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace smap.Data.Output
{
    public class DataOutput
    {
        public const int MaxLettersPerRow = 58;
        public const int MaxRowsPerPage = 48;
        public const int MaxLettersPerPage = MaxLettersPerRow * MaxRowsPerPage;
        
        private readonly byte[] _data;

        public DataOutput(byte[] data)
        {
            _data = data;
        }

        public string GetDataAsBase32String()
        {
            return Encoder.Encode(_data);
        }

        private MemoryStream GetQrCodeAsBmpImage(MetaData metaData)
        {
            var barcodeWriter = new BarcodeWriterPixelData()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions()
                {
                    ErrorCorrection = ErrorCorrectionLevel.Q
                }
            };

            var pixelData = barcodeWriter.Write(Encoder.EncodeBase64(metaData.ToBytes()));

            var bmpImage = ImageHelper.PixelDataToBmp(pixelData, 10);
            return new MemoryStream(bmpImage);
        }

        private void AddQrCodeToPage(MemoryStream qrCode, XGraphics gfx)
        {
            var image = XImage.FromStream(qrCode);
            gfx.DrawImage(image, 30, 30, 57, 57);
        }

        private void WriteTextOnPage(IEnumerable<string> textLines, XGraphics gfx)
        {
            var font = new XFont("Consolas", 12, XFontStyle.Regular);
            var rowIndex = 0;
            foreach (var rowData in textLines)
            {
                for (var i = 0; i < rowData.Length; i++)
                {
                    gfx.DrawString(rowData[i].ToString(), font, XBrushes.Black, 37 + i * 9, 30 + 57 + 14 + rowIndex * 15);                    
                }
                rowIndex++;
            }
        }

        public void SaveDataAsPdf(string outputFileName)
        {
            var base32 = GetDataAsBase32String();
            
            var pdfDocument = new PdfDocument();
            
            var pages = base32.Split(MaxLettersPerPage);
            var pageIndex = 1;
            foreach (var page in pages)
            {
                var qrCode = GetQrCodeAsBmpImage(new MetaData
                {
                    TotalDataSize = (uint) base32.Length,
                    DataChunkSize = (uint) page.Length,
                    ChunkId = (uint) pageIndex,
                    Checksum = Sha1Helper.ComputeSha1Checksum(page)
                });
                var pdfPage = pdfDocument.AddPage();
                var gfx = XGraphics.FromPdfPage(pdfPage);
                AddQrCodeToPage(qrCode, gfx);
                WriteTextOnPage(page.Split(MaxLettersPerRow), gfx);
                
                pageIndex++;
            }
            
            pdfDocument.Save(outputFileName);
        }

        public static CsvParser GetCsvParser(IEnumerable<CsvRow> rows)
        {
            var csvReader = new CsvParser(() =>
            {
                var memoryStream = new MemoryStream();
                var csvWriter = new CsvWriter(() => new StreamWriter(memoryStream, Encoding.Default, 4096, true));
                csvWriter.Write(rows);
                memoryStream.Seek(0, 0);
                return new StreamReader(memoryStream);
            });
            return csvReader;
        }
    }
}