using Display.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Helper.Crypto
{
    public static class HashHelper
    {
        public static string ComputeMd5ByContent(string content)
        {
            var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hashBytes = md5.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes);
        }

        public static string ComputeSha1ByPath(string filePath)
        {
            using FileStream stream = File.OpenRead(filePath);
            var sha = SHA1.Create();
            byte[] hash = sha.ComputeHash(stream);
            return Convert.ToHexString(hash);
        }

        public static string ComputeSha1ByContent(string content)
        {
            var hashDlg = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hashBytes = hashDlg.ComputeHash(bytes);

            return Convert.ToHexString(hashBytes);
        }

        public static async Task<string> ComputeSha1ByStream(Stream stream, CancellationToken cancellationToken)
        {
            using var sha = SHA1.Create();
            var hash = await sha.ComputeHashAsync(stream, cancellationToken);
            return Convert.ToHexString(hash);
        }

        public static async Task<string> ComputeSha1ByStream(Stream stream, CancellationToken cancellationToken, IProgress<long> progress, int bufferSize = 1024 * 1024)
        {
            using var sha = SHA1.Create();

            var hash = await sha.ComputeHashAsync(stream, progress, cancellationToken, bufferSize);

            return hash == null ? null : Convert.ToHexString(hash);
        }

        public static string ComputeSha1RangeByStream(FileStream stream, string signCheck)
        {
            var ranges = signCheck.Split("-");
            if (ranges.Length != 2) return string.Empty;

            if (!long.TryParse(ranges[0], out var startIndex) || !long.TryParse(ranges[1], out var endIndex)) return string.Empty;

            var bufferSize = (int)(endIndex - startIndex + 1);
            var buffer = new byte[bufferSize];

            stream.Seek(startIndex, SeekOrigin.Begin);
            var readSize = stream.Read(buffer, 0, bufferSize);
            if (readSize != bufferSize) Debug.WriteLine("读取的大小与要求的大小不一致，可能有问题");

            using var sha = SHA1.Create();
            var hash = sha.ComputeHash(buffer);
            return Convert.ToHexString(hash);
        }


        public static string ComputeSha1RangeByPath(string filePath, string signCheck)
        {
            var ranges = signCheck.Split("-");
            if (ranges.Length != 2) return string.Empty;

            if (!long.TryParse(ranges[0], out var startIndex) || !long.TryParse(ranges[1], out var endIndex)) return string.Empty;

            int bufferSize = (int)(endIndex - startIndex + 1);
            byte[] buffer = new byte[bufferSize];

            using FileStream stream = File.OpenRead(filePath);
            stream.Seek(startIndex, 0);
            var readSize = stream.Read(buffer, 0, bufferSize);

            var sha = SHA1.Create();
            var hash = sha.ComputeHash(buffer);
            return Convert.ToHexString(hash);
        }
    }
}
