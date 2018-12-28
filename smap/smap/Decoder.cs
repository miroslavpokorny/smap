using Cambia.BaseN;

namespace smap
{
    public static class Decoder
    {
        public static byte[] Decode(string data)
        {
            var bi = BaseConverter.Parse(data, Alphabet.Base32Alphabet);
            return bi.ToByteArray();
        }
    }
}