using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Display.Helper;

namespace Display.Data
{
    internal class m115
    {
        static readonly byte[] _gKts = { 0xf0, 0xe5, 0x69, 0xae, 0xbf, 0xdc, 0xbf, 0x8a, 0x1a, 0x45, 0xe8, 0xbe, 0x7d, 0xa6, 0x73, 0xb8, 0xde, 0x8f, 0xe7, 0xc4, 0x45, 0xda, 0x86, 0xc4, 0x9b, 0x64, 0x8b, 0x14, 0x6a, 0xb4, 0xf1, 0xaa, 0x38, 0x01, 0x35, 0x9e, 0x26, 0x69, 0x2c, 0x86, 0x00, 0x6b, 0x4f, 0xa5, 0x36, 0x34, 0x62, 0xa6, 0x2a, 0x96, 0x68, 0x18, 0xf2, 0x4a, 0xfd, 0xbd, 0x6b, 0x97, 0x8f, 0x4d, 0x8f, 0x89, 0x13, 0xb7, 0x6c, 0x8e, 0x93, 0xed, 0x0e, 0x0d, 0x48, 0x3e, 0xd7, 0x2f, 0x88, 0xd8, 0xfe, 0xfe, 0x7e, 0x86, 0x50, 0x95, 0x4f, 0xd1, 0xeb, 0x83, 0x26, 0x34, 0xdb, 0x66, 0x7b, 0x9c, 0x7e, 0x9d, 0x7a, 0x81, 0x32, 0xea, 0xb6, 0x33, 0xde, 0x3a, 0xa9, 0x59, 0x34, 0x66, 0x3b, 0xaa, 0xba, 0x81, 0x60, 0x48, 0xb9, 0xd5, 0x81, 0x9c, 0xf8, 0x6c, 0x84, 0x77, 0xff, 0x54, 0x78, 0x26, 0x5f, 0xbe, 0xe8, 0x1e, 0x36, 0x9f, 0x34, 0x80, 0x5c, 0x45, 0x2c, 0x9b, 0x76, 0xd5, 0x1b, 0x8f, 0xcc, 0xc3, 0xb8, 0xf5 };
        static readonly byte[] GKeyS = { 0x29, 0x23, 0x21, 0x5E };
        static readonly byte[] GKeyL = { 0x78, 0x06, 0xad, 0x4c, 0x33, 0x86, 0x5d, 0x18, 0x4c, 0x01, 0x3f, 0x46 };

        //static string privateKey = "MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAIyBQkvBZvSRh1bp\r\n97Iu+qA0ebCB5hiWhyy3xRyRDX7BpM4ocUJNXJFJvV4IollZoZrTyYHmUS79qyu4\r\n2j8eMVwpS9EXqfudjOjmM7SWLgh8Yp3GyjoUkhS0CR7ysDY8s65sfucCN38FXtPN\r\nk/bDQiVqdlVLvqfyA0N7vmXy2idBAgMBAAECgYA3BNqwDYDCXkZP+3haFtlfaI0K\r\nWCOBF1jBYwjVodtV+oANlnqbSu3nmqeDrf/c2yNUHIC41DaQHxcrHMyhkLIk2+d3\r\nvxi5bdmjCqzoeANQeTpPkKZFp3R+9pViLq2+I6TG2I8i6HhCtDs1SGwtG1sfp32z\r\nUosJEMqE7bekav2+0QJBAOmMpiywX6Sr2LID1a9dGNtj35xrbth9qsP1ZXNZLasW\r\nDwywJu8Kj15tdyaO7jhCEKCFAUhVe55tDtCnJ2+oXSUCQQCaAueJvVeidg7GNUkz\r\naOrLnMQZ7q/ODxpLAoJh5zXiKIkqYRhw/jMNJGazjeGdCynwzbKeOexgKPKJ6CD4\r\nBnztAkA27DCYCdI0M4V+N5Ck8MvLrC0F5+3lU4g5FRiKi8pFlaZsYXCGfoFAv5Vp\r\np+s1p7OpTB4FGLU9iIAXaXfItltRAkB/+c3gfP+nNibMuVaca6A/lYK2ccqQlagp\r\nkGo7ZF84EKr6FjizG+fcEdVteoZxcudk++hi5oru1NfFlKhgsTN5AkEAvwy3cCQX\r\nUZyLkc66DqNPizhn6oMOtRej0GVPBJY/3S/CUcX6aRMmkp3lZpx5/S9X7G6ao2iu\r\n8ukhdYrDE1qgcw==";

        private const string publicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCGhpgMD1okxLnUMCDNLCJwP/P0\r\nUHVlKQWLHPiPCbhgITZHcZim4mgxSWWb0SLDNZL9ta1HlErR6k02xrFyqtYzjDu2\r\nrGInUC0BCZOsln0a7wDwyOA43i5NO8LsNory6fEKbx7aT3Ji8TZCDAfDMbhxvxOf\r\ndPMBDjxP5X3zr7cWgwIDAQAB";

        public static string decode(byte[] srcBase64, byte[] keyBytes)
        {
            string rep;

            var tmp = asym_decode(srcBase64, srcBase64.Length);

            var tmp2 = sym_decode(tmp.Skip(16).Take(tmp.Length - 16).ToArray(), tmp.Length - 16, Encoding.ASCII.GetString(keyBytes), Encoding.ASCII.GetString(tmp.Skip(0).Take(16).ToArray()));

            rep = Encoding.ASCII.GetString(tmp2);

            return rep;
        }

        public static (byte[], byte[]) encode(string src, long tm)
        {
            var hashDlg = MD5.Create();
            var btext = Encoding.ASCII.GetBytes($"!@###@#{tm}DFDR@#@#");
            byte[] hashBytes = hashDlg.ComputeHash(btext);
            var key = Convert.ToHexString(hashBytes);
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var tmp = Encoding.ASCII.GetBytes(src);
            tmp = sym_encode(tmp, tmp.Length, key, string.Empty);
            byte[] partKeyBytes = keyBytes.Skip(0).Take(16).ToArray();

            byte[] newTmp = new byte[partKeyBytes.Length + tmp.Length];
            partKeyBytes.CopyTo(newTmp, 0);
            tmp.CopyTo(newTmp, partKeyBytes.Length);
            var reult = asym_encode(newTmp, newTmp.Length);

            return (reult, keyBytes);
        }

        static byte[] asym_decode(byte[] src, int srclen)
        {
            int m = 128;
            List<byte> ret = new();
            for (int i = 0; i < (srclen + m - 1) / m; i++)
            {
                int startIndex = i * m;
                int maxIndex = Math.Min((i + 1) * m, srclen);
                var message_slice = src.Skip(startIndex).Take(maxIndex - startIndex).ToArray();

                byte[] decryptedData = RSAPcks12Helper.DecryptWithPublicKey(Convert.FromBase64String(publicKey), message_slice);

                ret.AddRange(decryptedData.ToList());
            }

            byte[] bytes = new byte[ret.Count];
            for (int i = 0; i < ret.Count; i++)
            {
                bytes[i] = ret[i];
            }

            return bytes;
        }

        static byte[] asym_encode(byte[] src, int srclen)
        {
            int m = 128 - 11;
            List<byte> ret = new();
            for (int i = 0; i < (srclen + m - 1) / m; i++)
            {
                int startIndex = i * m;
                int maxIndex = Math.Min((i + 1) * m, srclen);
                var message = src.Skip(startIndex).Take(maxIndex - startIndex).ToArray();

                string encryptedContent = RSAPcks12Helper.EncryptWithPublicKey(publicKey, message);

                var item = Encoding.ASCII.GetBytes(encryptedContent);
                ret.AddRange(item.ToList());
            }

            byte[] bytes = new byte[ret.Count];
            for (int i = 0; i < ret.Count; i++)
            {
                bytes[i] = ret[i];
            }

            return bytes;
        }

        static byte[] sym_decode(byte[] src, int srclen, string key1, string key2)
        {
            var k1 = getkey(4, key1);
            var k2 = getkey(12, key2);
            var ret = xor115_enc(src, srclen, k2, 12);
            ret = ret.Reverse().ToArray();
            ret = xor115_enc(ret, srclen, k1, 4);

            return ret;
        }

        static byte[] sym_encode(byte[] src, int srclen, string key1, string key2)
        {
            var k1 = getkey(4, key1);
            var k2 = getkey(12, key2);
            var ret = xor115_enc(src, srclen, k1, 4);
            ret = ret.Reverse().ToArray();
            ret = xor115_enc(ret, srclen, k2, 12);

            return ret;
        }

        static byte[] getkey(int length, string key)
        {
            if (key != string.Empty)
            {
                List<int> results = new();
                for (int i = 0; i < length; i++)
                {
                    results.Add(key[i] + _gKts[length * i] & 0xff ^ _gKts[length * (length - 1 - i)]);
                }
                byte[] bytes = new byte[results.Count];
                for (int i = 0; i < results.Count; i++)
                {
                    bytes[i] = (byte)results[i];
                }
                return bytes;
            }
            if (length == 12)
            {
                return GKeyL;
            }

            return GKeyS;
        }

        static byte[] xor115_enc(byte[] src, int srclen, byte[] key, int keylen)
        {
            var mod4 = srclen % 4;
            List<int> ret = new();
            if (mod4 != 0)
            {
                for (int i = 0; i < mod4; i++)
                {
                    ret.Add(src[i] ^ key[i % keylen]);
                }
            }
            for (int i = 0; i < srclen; i++)
            {
                if (i < mod4) continue;
                ret.Add(src[i] ^ key[(i - mod4) % keylen]);
            }

            byte[] bytes = new byte[ret.Count];
            for (int i = 0; i < ret.Count; i++)
            {
                bytes[i] = (byte)ret[i];
            }


            return bytes;
        }

    }
}
