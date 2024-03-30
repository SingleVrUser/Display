using K4os.Compression.LZ4;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Display.Helper.Crypto
{
    internal static class UploadEncryptHelper
    {
        private const string Md5Salt = "Qclm8MGWUv59TnrR0XPg";
        private static readonly byte[] CrcSaltBytes = "^j>WD3Kr?J2gLFjD4W2y@"u8.ToArray();
        public static readonly byte[] ServerPubKey =
        [
            0x04, 0x57, 0xa2, 0x92, 0x57, 0xcd, 0x23, 0x20,
            0xe5, 0xd6, 0xd1, 0x43, 0x32, 0x2f, 0xa4, 0xbb,
            0x8a, 0x3c, 0xf9, 0xd3, 0xcc, 0x62, 0x3e, 0xf5,
            0xed, 0xac, 0x62, 0xb7, 0x67, 0x8a, 0x89, 0xc9,
            0x1a, 0x83, 0xba, 0x80, 0x0d, 0x61, 0x29, 0xf5,
            0x22, 0xd0, 0x34, 0xc8, 0x95, 0xdd, 0x24, 0x65,
            0x24, 0x3a, 0xdd, 0xc2, 0x50, 0x95, 0x3b, 0xee,
            0xba
        ];

        public static string GetSign(int userid, string fileId, string target, string userKey)
        {
            string sha1Str = $"{userid}{fileId}{target}0";
            var tmp = HashHelper.ComputeSha1ByContent(sha1Str).ToLower();

            sha1Str = $"{userKey}{tmp}000000";
            sha1Str = HashHelper.ComputeSha1ByContent(sha1Str);

            return sha1Str;
        }

        private static byte[] AesEncrypt(byte[] dataBytes, byte[] aesKey, byte[] aesIv)
        {
            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.Zeros;
            using var decryption = aes.CreateEncryptor(aes.Key, aes.IV);

            // 开始加密
            return decryption.TransformFinalBlock(dataBytes, 0, dataBytes.Length);
        }

        private static byte[] AesDecrypt(byte[] cipherText, byte[] aesKey, byte[] aesIv)
        {
            // Decrypt
            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.IV = aesIv;
            aes.Mode = CipherMode.CBC;

            // TODO Zero?
            aes.Padding = PaddingMode.None;

            using var decryption = aes.CreateDecryptor(aes.Key, aes.IV);

            return decryption.TransformFinalBlock(cipherText, 0, cipherText.Length);
        }

        public static byte[] GetData(Dictionary<string, string> dataForm, byte[] aesKey, byte[] aesIv)
        {
            var data = string.Join("&", dataForm.Select(x => $"{x.Key}={x.Value}"));
            var dataBytes = Encoding.UTF8.GetBytes(data);

            return AesEncrypt(dataBytes, aesKey, aesIv);
        }

        public static string GetKEc(byte[] publicKeyBytes30, long timestamp)
        {
            // 两个随机数
            var random = new Random();
            var r1 = (byte)random.Next(256);
            var r2 = (byte)random.Next(256);

            var tmp = new byte[44];

            // 0~14
            for (var i = 0; i < 15; i++)
            {
                tmp[i] = (byte)(publicKeyBytes30[i] ^ r1);
            }

            // 15
            tmp[15] = r1;
            var _115Bytes = BitConverter.GetBytes(115);
            // 16~19
            for (int i = 0; i < _115Bytes.Length; i++)
            {
                tmp[16 + i] = (byte)(r1 ^ _115Bytes[i]);
            }

            var timeBytes = BitConverter.GetBytes(timestamp);
            // 20~23
            for (int i = 0; i < timeBytes.Length; i++)
            {
                tmp[20 + i] = (byte)(r1 ^ timeBytes[i]);
            }

            // 24~38
            for (int i = 15; i < publicKeyBytes30.Length; i++)
            {
                tmp[i + 9] = (byte)(publicKeyBytes30[i] ^ r2);
            }

            // 39
            tmp[39] = r2;

            // 40
            tmp[40] = (byte)(1 ^ r2);
            // 41~43
            for (var i = 41; i < 44; i++)
            {
                tmp[i] = r2;
            }

            var crc32Bytes = new byte[65];
            CrcSaltBytes.CopyTo(crc32Bytes, 0);
            Array.Copy(tmp, 0, crc32Bytes, CrcSaltBytes.Length, tmp.Length);

            var bytes = Crc32Helper.CalcCrc32(crc32Bytes);
            var token = new byte[48];
            tmp.CopyTo(token, 0);

            // 44~48
            bytes.CopyTo(token, tmp.Length);

            var s = Convert.ToBase64String(token);

            return s;
        }

        public static string GetToken(string fileId, long fileSize, string signKey, string signVal, long timeSpan, int userId, string userIdMd5, string appVer)
        {
            // token(md5格式)
            var token = $"{Md5Salt}{fileId}{fileSize}{signKey}{signVal}{userId}{timeSpan}{userIdMd5}{appVer}";
            var tokenMd5 = HashHelper.ComputeMd5ByContent(token).ToLower();
            return tokenMd5;
        }

        public static T DecryptReceiveData<T>(byte[] data, byte[] aesKey, byte[] aesIv) where T : new()
        {
            var cipherText = new byte[data.Length - 12];

            Array.Copy(data, cipherText, cipherText.Length);

            var tail = new byte[12];
            Array.Copy(data, data.Length - 12, tail, 0, tail.Length);

            uint crc32 = Crc32Helper.Compute(CrcSaltBytes.Concat(tail.Take(8)).ToArray());
            var trueCrc32 = BitConverter.ToUInt32(tail.Skip(8).Take(4).ToArray(), 0);

            if (crc32 != trueCrc32)
            {
                Debug.WriteLine("无效的_crc_salt");
                return default;
            }

            var plaintext = AesDecrypt(cipherText, aesKey, aesIv);

            // Decompress
            for (int i = 0; i < 4; i++)
                tail[i] ^= tail[7];

            // 解密后的大小
            var dstSize = BitConverter.ToInt32(tail.Take(4).ToArray(), 0);

            // 解密前的大小
            var srcSize = BitConverter.ToUInt16(plaintext.Take(2).ToArray(), 0);

            if (srcSize > plaintext.Length)
            {
                Debug.WriteLine("Can't unCompress data");
                return default;
            }

            // 需要解密的信息
            var compressedData = plaintext.Skip(2).Take(srcSize).ToArray();

            var target = new byte[dstSize]; // or source.Length * 255 to be safe

            // 解密
            var decodedCount = LZ4Codec.Decode(
                compressedData,
                target);

            var content = Encoding.Default.GetString(target);

            Debug.WriteLine($"最终结果: {content}");

            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
