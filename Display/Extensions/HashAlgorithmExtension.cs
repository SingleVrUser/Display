using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Extensions
{
    internal static class HashAlgorithmExtension
    {
        public static async Task<byte[]> ComputeHashAsync(this HashAlgorithm hashAlgorithm, Stream stream, CancellationToken cancellationToken = default,
            IProgress<long> progress = null,
            int bufferSize = 1024 * 1024)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            long totalBytesRead = 0;

            var readAheadBuffer = new byte[bufferSize];
            var readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0,
                readAheadBuffer.Length, cancellationToken);
            totalBytesRead += readAheadBytesRead;

            while (readAheadBytesRead != 0)
            {
                // 上一个 byteRead
                var bytesRead = readAheadBytesRead;
                // 上一个 buffer
                var buffer = readAheadBuffer;
                readAheadBuffer = new byte[bufferSize];

                readAheadBytesRead = await stream.ReadAsync(readAheadBuffer, 0,
                    readAheadBuffer.Length, cancellationToken);
                totalBytesRead += readAheadBytesRead;

                if (readAheadBytesRead == 0)
                    hashAlgorithm.TransformFinalBlock(buffer, 0, bytesRead);
                else
                    hashAlgorithm.TransformBlock(buffer, 0, bytesRead, buffer, 0);

                progress?.Report(totalBytesRead);

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
            }

            return hashAlgorithm.Hash;
        }
    }
}
