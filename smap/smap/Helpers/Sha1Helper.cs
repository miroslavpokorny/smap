using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace smap.Helpers
{
    public static class Sha1Helper
    {
        private static readonly SHA1CryptoServiceProvider _sha1CryptoServiceProvider;
        
        static Sha1Helper()
        {
            _sha1CryptoServiceProvider = new SHA1CryptoServiceProvider();
        }
        
        public static byte[] ComputeSha1Checksum(string data)
        {
            return _sha1CryptoServiceProvider.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        public static bool IsChecksumCorrect(string data, byte[] checksum)
        {
            return ComputeSha1Checksum(data).ToList().SequenceEqual(checksum);
        }
        
    }
}