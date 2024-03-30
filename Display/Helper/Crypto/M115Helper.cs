using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Display.Helper.Crypto;

internal static class M115Helper
{
    private static readonly byte[] GKts = [0xf0, 0xe5, 0x69, 0xae, 0xbf, 0xdc, 0xbf, 0x8a, 0x1a, 0x45, 0xe8, 0xbe, 0x7d, 0xa6, 0x73, 0xb8, 0xde, 0x8f, 0xe7, 0xc4, 0x45, 0xda, 0x86, 0xc4, 0x9b, 0x64, 0x8b, 0x14, 0x6a, 0xb4, 0xf1, 0xaa, 0x38, 0x01, 0x35, 0x9e, 0x26, 0x69, 0x2c, 0x86, 0x00, 0x6b, 0x4f, 0xa5, 0x36, 0x34, 0x62, 0xa6, 0x2a, 0x96, 0x68, 0x18, 0xf2, 0x4a, 0xfd, 0xbd, 0x6b, 0x97, 0x8f, 0x4d, 0x8f, 0x89, 0x13, 0xb7, 0x6c, 0x8e, 0x93, 0xed, 0x0e, 0x0d, 0x48, 0x3e, 0xd7, 0x2f, 0x88, 0xd8, 0xfe, 0xfe, 0x7e, 0x86, 0x50, 0x95, 0x4f, 0xd1, 0xeb, 0x83, 0x26, 0x34, 0xdb, 0x66, 0x7b, 0x9c, 0x7e, 0x9d, 0x7a, 0x81, 0x32, 0xea, 0xb6, 0x33, 0xde, 0x3a, 0xa9, 0x59, 0x34, 0x66, 0x3b, 0xaa, 0xba, 0x81, 0x60, 0x48, 0xb9, 0xd5, 0x81, 0x9c, 0xf8, 0x6c, 0x84, 0x77, 0xff, 0x54, 0x78, 0x26, 0x5f, 0xbe, 0xe8, 0x1e, 0x36, 0x9f, 0x34, 0x80, 0x5c, 0x45, 0x2c, 0x9b, 0x76, 0xd5, 0x1b, 0x8f, 0xcc, 0xc3, 0xb8, 0xf5];
    private static readonly byte[] GKeyS = [0x29, 0x23, 0x21, 0x5E];
    private static readonly byte[] GKeyL = [0x78, 0x06, 0xad, 0x4c, 0x33, 0x86, 0x5d, 0x18, 0x4c, 0x01, 0x3f, 0x46];

    //static string privateKey = "MIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAIyBQkvBZvSRh1bp\r\n97Iu+qA0ebCB5hiWhyy3xRyRDX7BpM4ocUJNXJFJvV4IollZoZrTyYHmUS79qyu4\r\n2j8eMVwpS9EXqfudjOjmM7SWLgh8Yp3GyjoUkhS0CR7ysDY8s65sfucCN38FXtPN\r\nk/bDQiVqdlVLvqfyA0N7vmXy2idBAgMBAAECgYA3BNqwDYDCXkZP+3haFtlfaI0K\r\nWCOBF1jBYwjVodtV+oANlnqbSu3nmqeDrf/c2yNUHIC41DaQHxcrHMyhkLIk2+d3\r\nvxi5bdmjCqzoeANQeTpPkKZFp3R+9pViLq2+I6TG2I8i6HhCtDs1SGwtG1sfp32z\r\nUosJEMqE7bekav2+0QJBAOmMpiywX6Sr2LID1a9dGNtj35xrbth9qsP1ZXNZLasW\r\nDwywJu8Kj15tdyaO7jhCEKCFAUhVe55tDtCnJ2+oXSUCQQCaAueJvVeidg7GNUkz\r\naOrLnMQZ7q/ODxpLAoJh5zXiKIkqYRhw/jMNJGazjeGdCynwzbKeOexgKPKJ6CD4\r\nBnztAkA27DCYCdI0M4V+N5Ck8MvLrC0F5+3lU4g5FRiKi8pFlaZsYXCGfoFAv5Vp\r\np+s1p7OpTB4FGLU9iIAXaXfItltRAkB/+c3gfP+nNibMuVaca6A/lYK2ccqQlagp\r\nkGo7ZF84EKr6FjizG+fcEdVteoZxcudk++hi5oru1NfFlKhgsTN5AkEAvwy3cCQX\r\nUZyLkc66DqNPizhn6oMOtRej0GVPBJY/3S/CUcX6aRMmkp3lZpx5/S9X7G6ao2iu\r\n8ukhdYrDE1qgcw==";
    private const string PublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCGhpgMD1okxLnUMCDNLCJwP/P0\r\nUHVlKQWLHPiPCbhgITZHcZim4mgxSWWb0SLDNZL9ta1HlErR6k02xrFyqtYzjDu2\r\nrGInUC0BCZOsln0a7wDwyOA43i5NO8LsNory6fEKbx7aT3Ji8TZCDAfDMbhxvxOf\r\ndPMBDjxP5X3zr7cWgwIDAQAB";

    public static string Decode(byte[] srcBase64, byte[] keyBytes)
    {
        string rep;

        var tmp = AsymcDecode(srcBase64, srcBase64.Length);

        var tmp2 = SymDecode(tmp.Skip(16).Take(tmp.Length - 16).ToArray(), tmp.Length - 16, Encoding.ASCII.GetString(keyBytes), Encoding.ASCII.GetString(tmp.Skip(0).Take(16).ToArray()));

        rep = Encoding.ASCII.GetString(tmp2);

        return rep;
    }

    public static (byte[], byte[]) Encode(string src, long tm)
    {
        var key = HashHelper.ComputeMd5ByContent($"!@###@#{tm}DFDR@#@#");

        var keyBytes = Encoding.ASCII.GetBytes(key);

        var tmp = Encoding.ASCII.GetBytes(src);
        tmp = SymEncode(tmp, tmp.Length, key, string.Empty);
        byte[] partKeyBytes = keyBytes.Skip(0).Take(16).ToArray();

        byte[] newTmp = new byte[partKeyBytes.Length + tmp.Length];
        partKeyBytes.CopyTo(newTmp, 0);
        tmp.CopyTo(newTmp, partKeyBytes.Length);

        return (AsymEncode(newTmp, newTmp.Length), keyBytes);
    }

    private static byte[] AsymcDecode(byte[] src, int srcLen)
    {
        var m = 128;
        List<byte> ret = [];
        for (var i = 0; i < (srcLen + m - 1) / m; i++)
        {
            var startIndex = i * m;
            var maxIndex = Math.Min((i + 1) * m, srcLen);
            var messageSlice = src.Skip(startIndex).Take(maxIndex - startIndex).ToArray();

            var decryptedData = RsaPcks12Helper.DecryptWithPublicKey(Convert.FromBase64String(PublicKey), messageSlice);

            ret.AddRange(decryptedData.ToList());
        }

        var bytes = new byte[ret.Count];
        for (var i = 0; i < ret.Count; i++)
        {
            bytes[i] = ret[i];
        }

        return bytes;
    }

    private static byte[] AsymEncode(byte[] src, int srcLen)
    {
        var m = 128 - 11;
        List<byte> ret = [];
        for (var i = 0; i < (srcLen + m - 1) / m; i++)
        {
            var startIndex = i * m;
            var maxIndex = Math.Min((i + 1) * m, srcLen);
            var message = src.Skip(startIndex).Take(maxIndex - startIndex).ToArray();

            var encryptedContent = RsaPcks12Helper.EncryptWithPublicKey(PublicKey, message);

            var item = Encoding.ASCII.GetBytes(encryptedContent);
            ret.AddRange(item.ToList());
        }

        var bytes = new byte[ret.Count];
        for (var i = 0; i < ret.Count; i++)
        {
            bytes[i] = ret[i];
        }

        return bytes;
    }

    private static byte[] SymDecode(byte[] src, int srcLen, string key1, string key2)
    {
        var k1 = GetKey(4, key1);
        var k2 = GetKey(12, key2);
        var ret = Xor115Enc(src, srcLen, k2, 12);
        ret = ret.Reverse().ToArray();
        ret = Xor115Enc(ret, srcLen, k1, 4);

        return ret;
    }

    private static byte[] SymEncode(byte[] src, int srcLen, string key1, string key2)
    {
        var k1 = GetKey(4, key1);
        var k2 = GetKey(12, key2);
        var ret = Xor115Enc(src, srcLen, k1, 4);
        ret = ret.Reverse().ToArray();
        ret = Xor115Enc(ret, srcLen, k2, 12);

        return ret;
    }

    private static byte[] GetKey(int length, string key)
    {
        if (key == string.Empty) return length == 12 ? GKeyL : GKeyS;

        List<int> results = new();
        for (var i = 0; i < length; i++)
        {
            results.Add(key[i] + GKts[length * i] & 0xff ^ GKts[length * (length - 1 - i)]);
        }
        var bytes = new byte[results.Count];
        for (var i = 0; i < results.Count; i++)
        {
            bytes[i] = (byte)results[i];
        }
        return bytes;

    }

    private static byte[] Xor115Enc(byte[] src, int srcLen, byte[] key, int keyLen)
    {
        var mod4 = srcLen % 4;
        List<int> ret = new();
        if (mod4 != 0)
        {
            for (var i = 0; i < mod4; i++)
            {
                ret.Add(src[i] ^ key[i % keyLen]);
            }
        }
        for (var i = 0; i < srcLen; i++)
        {
            if (i < mod4) continue;
            ret.Add(src[i] ^ key[(i - mod4) % keyLen]);
        }

        var bytes = new byte[ret.Count];
        for (var i = 0; i < ret.Count; i++)
        {
            bytes[i] = (byte)ret[i];
        }


        return bytes;
    }

}