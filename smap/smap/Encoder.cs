using System.Numerics;
using Cambia.BaseN;

namespace smap
{
    public static class Encoder
    {
        public static string Encode(byte[] data)
        {
            return EncodeBase32(data);
        }

        public static string EncodeBase32(byte[] data)
        {
            var bi = new BigInteger(data);
            return BaseConverter.ToBaseN(bi, Alphabet.Base32Alphabet);
        }

        public static string EncodeBase64(byte[] data)
        {
            var bi = new BigInteger(data);
            return BaseConverter.ToBaseN(bi, BaseNAlphabet.Base64_RFC4648);
        }
    }
}