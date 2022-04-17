using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// ReSharper disable InvertIf

namespace Kagami.Utils
{
    public static class Util
    {
        public static double Bytes2MiB(long bytes, int round)
            => Math.Round(bytes / 1048576.0, round);

        /// <summary>
        /// Convert bv into av
        /// </summary>
        /// <param name="bvCode"></param>
        /// <returns></returns>
        public static string Bv2Av(string bvCode)
        {
            const long xor = 177451812L;
            const long add = 100618342136696320L;
            const string table = "fZodR9XQDSUm21yCkr6" +
                                 "zBqiveYah8bt4xsWpHn" +
                                 "JE7jL5VG3guMTKNPAwcF";

            var sed = new byte[] {11, 10, 3, 8, 4, 6, 2, 9, 5, 7};
            var chars = new Dictionary<char, int>();
            {
                for (var i = 0; i < table.Length; ++i)
                    chars.Add(table[i], i);
            }

            try
            {
                var r = 0L;
                for (var i = 0; i < sed.Length; i++)
                {
                    r += chars[bvCode[sed[i]]]
                         * (long) Math.Pow(table.Length, i);
                }

                var result = r - add ^ xor;
                return result is > 10000000000 or < 0 ? "" : $"av{result}";
            }

            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Download file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="header"></param>
        /// <param name="timeout"></param>
        /// <param name="limitLen"></param>
        /// <returns></returns>
        public static async Task<byte[]> Download(string url, 
            Dictionary<string, string> header = null,
            int timeout = 8000, int limitLen = 0)
        {
            // Create request
            var request = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
            {
                Timeout = new TimeSpan(0, 0, 0, timeout)
            };
            // Default useragent
            request.DefaultRequestHeaders.Add("User-Agent", new[]
            {
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) " ,
                "AppleWebKit/537.36 (KHTML, like Gecko) " ,
                "Chrome/94.0.4606.31 Safari/537.36 " ,
                "Kagami/1.0.0 (Konata Project)"
            });
            // Append request header
            if (header is not null)
                foreach (var (k, v) in header)
                    request.DefaultRequestHeaders.Add(k, v);

            // Open response stream
            var response = await request.GetByteArrayAsync(url);

            // length limitation
            if (limitLen is not 0)
                // Decline streaming transport
                if (response.LongLength > limitLen || response.LongLength is 0)
                    return null;

            // Receive the response data
            return response;
        }

        /// <summary>
        /// Get meta data
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetMetaData(string keys, string html)
        {
            var metaDict = new Dictionary<string, string>();

            foreach (var i in keys.Split("|"))
            {
                var pattern = i + @"=""(.*?)""(.|\s)*?content=""(.*?)"".*?>";

                // Match results
                foreach (Match j in Regex.Matches(html, pattern, RegexOptions.Multiline))
                {
                    metaDict.TryAdd(j.Groups[1].Value, j.Groups[3].Value);
                }
            }

            return metaDict;
        }

        /// <summary>
        /// Can I do
        /// </summary>
        /// <param name="factor">Probability scale</param>
        /// <returns></returns>
        public static bool CanIDo(double factor = 0.5f)
            => new Random().NextDouble() >= (1 - factor);
    }
}
