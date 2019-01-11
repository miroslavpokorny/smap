using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using CSharpx;
using smap.Data;
using smap.Data.Input;
using smap.Data.Output;
using smap.Helpers;
using SharpLearning.Containers.Matrices;
using SharpLearning.InputOutput.Csv;
using SharpLearning.Neural.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SmapCommon;
using SmapCommon.Extensions;
using Decoder = smap.Data.Decoder;

namespace smap
{
    public class Application
    {
        private Options _options;
        private IEnumerable<Error> _error;
        private QrReader _qrReader;
        private Stopwatch _stopwatch;
        private Dictionary<string, int> _columnNameToIndex;

        public Application(string[] args)
        {
            Configure();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options => _options = options)
                .WithNotParsed(errs => _error = errs);
            _qrReader = new QrReader();
        }
        
        public void Main()
        {
            if (_error != null)
            {
                _error.ForEach(err =>
                {
                    Console.WriteLine(err.ToString());
                });
                return;
            }

            _stopwatch = Stopwatch.StartNew();
            switch (_options.Mode)
            {
                case ProgramMode.Encode:
                    ProcessEncode();
                    break;
                case ProgramMode.Decode:
                    ProcessDecode();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Console.WriteLine($"Program done in {_stopwatch.Elapsed}");
        }

        private void ProcessEncode()
        {
            Console.WriteLine($"Encoding file {_options.Input}");
            var dataOutput = new DataOutput(File.ReadAllBytes(_options.Input));
            var extension = Path.HasExtension(_options.Output) ? "" : ".pdf";
            var fileName = $"{_options.Output}{extension}";
            dataOutput.SaveDataAsPdf(fileName);
        }

        private void ProcessDecode()
        {
            Console.WriteLine($"Decoding images in {_options.Input}");
            _columnNameToIndex = new Dictionary<string, int> {["class"] = 0};
            for (var i = 1; i <= 1024; i++)
            {
                _columnNameToIndex[$"pixel{i}"] = i;
            }
            var chunks = new List<DataChunk>();
            foreach (var fileName in Directory.GetFiles(_options.Input))
            {
                chunks.Add(DecodeFile($"{fileName}"));
            }

            if (chunks.Count == 0)
            {
                Console.WriteLine("No files found to decode");
                return;
            }

            var totalChunks = chunks[0].TotalChunks;

            if (chunks.Count != totalChunks)
            {
                Console.WriteLine("Some data chunk is missing file could not be restored");
                return;
            }
            
            chunks.Sort((a, b) => a.ChunkId < b.ChunkId ? -1 : a.ChunkId > b.ChunkId ? 1 : 0);

            Console.WriteLine("Writing decoded file");
            var stringBuilder = new StringBuilder();
            foreach (var dataChunk in chunks)
            {
                stringBuilder.Append(dataChunk.Data);
            }

            var decodedBytes = DecodeLetters(stringBuilder.ToString());
            File.WriteAllBytes($"{_options.Output}/decoded", decodedBytes);            
        }

        private DataChunk DecodeFile(string fileName)
        {
            var startAt = _stopwatch.Elapsed;
            Console.WriteLine($"Decoding file {fileName}");
            var dataChunk = ReadDataFromPage(fileName);
            Console.WriteLine($"File {fileName} decoded in {_stopwatch.Elapsed - startAt}");
            return dataChunk;
        }

        private QrReaderData ReadQrCode(string fileName)
        {
            Console.WriteLine("Reading QRCode");
            return _qrReader.ReadQrCode(fileName);
        }

        private DataChunk ReadDataFromPage(string fileName)
        {
            var qrCode = ReadQrCode(fileName);
            
            using (var image = Image.Load(fileName))
            {
                Console.WriteLine("Rotating and cropping page image");
                ProcessImageGetContentFromPage(image, qrCode);
                Console.WriteLine("Extracting letters image from page");
                var lettersCollection = ProcessImageGetLettersFromPage(image, qrCode);
                Console.WriteLine("Classifying extracted letters");
                var decodedLetters = DecodeExtractedLetters(lettersCollection, qrCode);
                var dataChunk = new DataChunk
                {
                    ChunkId = qrCode.MetaData.ChunkId,
                    TotalChunks = (uint) Math.Ceiling(qrCode.MetaData.TotalDataSize / (double) DataOutput.MaxLettersPerPage)
                };
                if (decodedLetters.IsChecksumOk)
                {
                    Console.WriteLine("Decoding extracted letters");
                    File.WriteAllLines($"{_options.Output}/{Path.GetFileName(fileName)}.part{qrCode.MetaData.ChunkId}.txt", decodedLetters.Letters.Split(DataOutput.MaxLettersPerRow));
                    dataChunk.Data = decodedLetters.Letters;
                }
                else
                {
                    Console.WriteLine("Classifying has failed => checksum is incorrect you must reconstruct data manually");
                    File.WriteAllLines($"{_options.Output}/chunk_{qrCode.MetaData.ChunkId}_{Path.GetFileName(fileName)}.error",
                        decodedLetters.Letters.Split(DataOutput.MaxRowsPerPage));
                }

                return dataChunk;
            }
        }

        private byte[] DecodeLetters(string data)
        {
            return Decoder.Decode(data);
        }

        private void ProcessImageGetContentFromPage(Image<Rgba32> image, QrReaderData qrData)
        {
            image.Mutate(x =>
                x.Crop(new SixLabors.Primitives.Rectangle(0, qrData.QrCodeBottomPositionY,
                        x.GetCurrentSize().Width - 1, x.GetCurrentSize().Height - qrData.QrCodeBottomPositionY - 1))
                    .Rotate((float) qrData.PageRotation).BackgroundColor(Rgba32.White));
            var contentArea = image.GetContentArea();
            image.Mutate(x => x.Crop(contentArea));
        }

        private IEnumerable<IEnumerable<CsvRow>> ProcessImageGetLettersFromPage(Image<Rgba32> image, QrReaderData qrData)
        {
            var rowsPerPage = (int) Math.Ceiling(qrData.MetaData.DataChunkSize / (double) DataOutput.MaxLettersPerRow);
                var letters = new List<CsvRow>((int) qrData.MetaData.DataChunkSize);
                var lettersFallback = new List<CsvRow>((int) qrData.MetaData.DataChunkSize);
                var nextY = 0;
                for (var y = 0; y < rowsPerPage; y++)
                {
                    var nextRowContentArea = image.GetNextRowContentArea(nextY);
                    nextY = nextRowContentArea.Y + nextRowContentArea.Height;

                    var rowImage = image.Clone(img => img.Crop(nextRowContentArea));

                    var currentRowLetters = y == rowsPerPage - 1
                        ? qrData.MetaData.DataChunkSize - DataOutput.MaxLettersPerRow * y : DataOutput.MaxLettersPerRow;
                    var nextX = 0;
                    for (var x = 0; x < currentRowLetters; x++)
                    {
                        var nextLetterContentArea = rowImage.GetNextLetterContentArea(nextX);
                        nextX = nextLetterContentArea.X + nextLetterContentArea.Width;
                        
                        var letterImage = rowImage.Clone(img =>
                            img.Crop(nextLetterContentArea));

                        using (var memoryStream = new MemoryStream())
                        {
                            letterImage.SaveAsBmp(memoryStream);
                            memoryStream.Seek(0, 0);
                            letters.Add(new CsvRow(_columnNameToIndex, LoadImageHelper.FileAsData(memoryStream)));                            
                            lettersFallback.Add(new CsvRow(_columnNameToIndex, LoadImageHelper.FileAsData(memoryStream, threshold: 90)));                            
                        }
                    }
                }

                return new List<IEnumerable<CsvRow>> {letters, lettersFallback};
        }
        
        private DecodedLetters DecodeExtractedLetters(IEnumerable<IEnumerable<CsvRow>> lettersCollections, QrReaderData qrData)
        {
            string currentResult = null;
            var isChecksumCorrect = false;
            foreach (var letters in lettersCollections)
            {
                var csvReader = DataOutput.GetCsvParser(letters);
                var targetName = "class";
                var featureNames = csvReader.EnumerateRows(c => c != targetName).First().ColumnNameToIndex.Keys.ToArray();        
                var testObservations = csvReader.EnumerateRows(featureNames).ToF64Matrix();
                testObservations.Map(p => p / 255);
                
                var model = ClassificationNeuralNetModel.Load(() => new StreamReader("network.xml"));
                var predictions = model.Predict(testObservations);
                
                var stringBuilder = new StringBuilder();
                foreach (var prediction in predictions)
                {
                    stringBuilder.Append(Math.Abs(prediction - -1) < 0.01 ? '-' : Alphabet.Base32Alphabet.ToString()[(int) prediction]);
                }

                currentResult = stringBuilder.ToString();

                isChecksumCorrect = Sha1Helper.IsChecksumCorrect(currentResult, qrData.MetaData.Checksum);

                if (isChecksumCorrect)
                {
                    break;
                }
            }

            return new DecodedLetters {Letters = currentResult, IsChecksumOk = isChecksumCorrect};
        }

        private void Configure()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}