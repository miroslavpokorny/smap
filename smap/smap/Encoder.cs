using System.Numerics;
using Cambia.BaseN;

namespace smap
{
    public static class Encoder
    {
        public static string Encode(byte[] data)
        {
            var bi = new BigInteger(data);
            return BaseConverter.ToBaseN(bi, Alphabet.Base32Alphabet);
        }
    }
}