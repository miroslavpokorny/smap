using System;
using System.Collections.Generic;
using System.Linq;

namespace SmapCommon.Extensions
{
    public static class StringExtensions
    {
        public static IEnumerable<string> Split(this string str, int chunkSize)
        {
            return Enumerable.Range(0, (int) Math.Ceiling(str.Length / (double) chunkSize)).Select(i => str.Substring(i * chunkSize, (i+1) * chunkSize > str.Length ? str.Length % chunkSize : chunkSize));
        }
    }
}