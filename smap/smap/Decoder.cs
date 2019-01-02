using Cambia.BaseN;

namespace smap
{
    public static class Decoder
    {
        public static byte[] Decode(string data)
        {
            return DecodeBase32(data);
        }

        public static byte[] DecodeBase32(string data)
        {
            var bi = BaseConverter.Parse(data, Alphabet.Base32Alphabet);
            return bi.ToByteArray();
        }

        public static byte[] DecodeBase64(string data)
        {
            var bi = BaseConverter.Parse(data, BaseNAlphabet.Base64_RFC4648);
            return bi.ToByteArray();
        }
    }
}