using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Display.Extensions
{
    internal static class HashAlgorithmExtension
    {
        public static async Task<byte[]> ComputeHashAsync(this HashAlgorithm hashAlgorithm, Stream stream,
            IProgress<long> progress, CancellationToken cancellationToken = default,
            int bufferSize = 1024 * 1024)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            var buffer = new byte[bufferSize];
            var streamLength = stream.Length;
            while (true)
            {
                // 必须加.ConfigureAwait(false)，不然异步运行一个以上，UI会卡
                var read = await stream.ReadAsync(buffer, 0, bufferSize, cancellationToken).ConfigureAwait(false);
                if (stream.Position == streamLength)
                {
                    hashAlgorithm.TransformFinalBlock(buffer, 0, read);

                    progress?.Report(stream.Position);
                    break;
                }

                hashAlgorithm.TransformBlock(buffer, 0, read, default, default);

                progress?.Report(stream.Position);

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }
            }
            return hashAlgorithm.Hash;
        }
    }
}
