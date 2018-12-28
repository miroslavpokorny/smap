using System.IO;
using System.Text;

namespace smap
{
    public class MetaData
    {
        public uint TotalDataSize { get; set; }
        public uint DataChunkSize { get; set; }
        public uint ChunkId { get; set; }
        public byte[] Checksum { get; set; }

        private const int Sha1ByteSize = 160 / 8;

        private const int MetaDataSize = Sha1ByteSize + 3 * sizeof(uint);

        public byte[] ToBytes()
        {
            var memoryStream = new MemoryStream(MetaDataSize);
            using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.Default, true))
            {
                binaryWriter.Write(TotalDataSize);
                binaryWriter.Write(DataChunkSize);
                binaryWriter.Write(ChunkId);
                binaryWriter.Write(Checksum);
            }

            memoryStream.Seek(0, 0);
            using (var binaryReader = new BinaryReader(memoryStream))
            {
                return binaryReader.ReadBytes(MetaDataSize);
            }
        }
        
        public void Parse(byte[] data)
        {
            var memoryStream = new MemoryStream(data);
            using (var binaryReader = new BinaryReader(memoryStream))
            {
                TotalDataSize = binaryReader.ReadUInt32();
                DataChunkSize = binaryReader.ReadUInt32();
                ChunkId = binaryReader.ReadUInt32();
                Checksum = binaryReader.ReadBytes(Sha1ByteSize);
            }
        }
    }
}