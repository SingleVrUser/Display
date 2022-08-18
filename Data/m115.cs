using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    internal class m115
    {
        static byte[] g_kts = { 0xF0, 0xE5, 0x69, 0xAE, 0xBF, 0xDC, 0xBF, 0x5A, 0x1A, 0x45, 0xE8, 0xBE, 0x7D, 0xA6, 0x73, 0x88, 0xDE, 0x8F, 0xE7, 0xC4, 0x45, 0xDA, 0x86, 0x94, 0x9B, 0x69, 0x92, 0x0B, 0x6A, 0xB8, 0xF1, 0x7A, 0x38, 0x06, 0x3C, 0x95, 0x26, 0x6D, 0x2C, 0x56, 0x00, 0x70, 0x56, 0x9C, 0x36, 0x38, 0x62, 0x76, 0x2F, 0x9B, 0x5F, 0x0F, 0xF2, 0xFE, 0xFD, 0x2D, 0x70, 0x9C, 0x86, 0x44, 0x8F, 0x3D, 0x14, 0x27, 0x71, 0x93, 0x8A, 0xE4, 0x0E, 0xC1, 0x48, 0xAE, 0xDC, 0x34, 0x7F, 0xCF, 0xFE, 0xB2, 0x7F, 0xF6, 0x55, 0x9A, 0x46, 0xC8, 0xEB, 0x37, 0x77, 0xA4, 0xE0, 0x6B, 0x72, 0x93, 0x7E, 0x51, 0xCB, 0xF1, 0x37, 0xEF, 0xAD, 0x2A, 0xDE, 0xEE, 0xF9, 0xC9, 0x39, 0x6B, 0x32, 0xA1, 0xBA, 0x35, 0xB1, 0xB8, 0xBE, 0xDA, 0x78, 0x73, 0xF8, 0x20, 0xD5, 0x27, 0x04, 0x5A, 0x6F, 0xFD, 0x5E, 0x72, 0x39, 0xCF, 0x3B, 0x9C, 0x2B, 0x57, 0x5C, 0xF9, 0x7C, 0x4B, 0x7B, 0xD2, 0x12, 0x66, 0xCC, 0x77, 0x09, 0xA6 };
        static byte[] g_key_s = { 0x29, 0x23, 0x21, 0x5E };
        static byte[] g_key_l = { 0x42, 0xDA, 0x13, 0xBA, 0x78, 0x76, 0x8D, 0x37, 0xE8, 0xEE, 0x04, 0x91 };

        static string xmlPrivateKey = "<RSAKeyValue><Modulus>jIFCS8Fm9JGHVun3si76oDR5sIHmGJaHLLfFHJENfsGkzihxQk1ckUm9XgiiWVmhmtPJgeZRLv2rK7jaPx4xXClL0Rep+52M6OYztJYuCHxincbKOhSSFLQJHvKwNjyzrmx+5wI3fwVe082T9sNCJWp2VUu+p/IDQ3u+ZfLaJ0E=</Modulus><Exponent>AQAB</Exponent><P>6YymLLBfpKvYsgPVr10Y22PfnGtu2H2qw/Vlc1ktqxYPDLAm7wqPXm13Jo7uOEIQoIUBSFV7nm0O0Kcnb6hdJQ==</P><Q>mgLnib1XonYOxjVJM2jqy5zEGe6vzg8aSwKCYec14iiJKmEYcP4zDSRms43hnQsp8M2ynjnsYCjyiegg+AZ87Q==</Q><DP>NuwwmAnSNDOFfjeQpPDLy6wtBeft5VOIORUYiovKRZWmbGFwhn6BQL+VaafrNaezqUweBRi1PYiAF2l3yLZbUQ==</DP><DQ>f/nN4Hz/pzYmzLlWnGugP5WCtnHKkJWoKZBqO2RfOBCq+hY4sxvn3BHVbXqGcXLnZPvoYuaK7tTXxZSoYLEzeQ==</DQ><InverseQ>vwy3cCQXUZyLkc66DqNPizhn6oMOtRej0GVPBJY/3S/CUcX6aRMmkp3lZpx5/S9X7G6ao2iu8ukhdYrDE1qgcw==</InverseQ><D>NwTasA2Awl5GT/t4WhbZX2iNClgjgRdYwWMI1aHbVfqADZZ6m0rt55qng63/3NsjVByAuNQ2kB8XKxzMoZCyJNvnd78YuW3Zowqs6HgDUHk6T5CmRad0fvaVYi6tviOkxtiPIuh4QrQ7NUhsLRtbH6d9s1KLCRDKhO23pGr9vtE=</D></RSAKeyValue>";
        
        static string xmlPublicKey = "<RSAKeyValue><Modulus>0d61pnmJ0QpcCwQdEatHZZvDKz5lqS+SNt0swhQKDaSV+g5++HX2+stNmB9eXjul5ngR3XO3pBBSVCIvrMIGp06JESFUUa/89ev96Ie8mttnXAKoNfdcvnemA6+CBoP0OefO1CiTI9+ckFV2nhV5i5RT6JBYs+Lj8YawewRqmW0=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

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

                byte[] decryptedData;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(xmlPrivateKey);
                    decryptedData = rsa.Decrypt(message_slice, false);
                }

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

                string encryptedContent = string.Empty;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                {
                    rsa.FromXmlString(xmlPublicKey);
                    byte[] encryptedData = rsa.Encrypt(message, false);
                    encryptedContent = Convert.ToBase64String(encryptedData);
                }

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
                    results.Add(key[i] + g_kts[length * i] & 0xff ^ g_kts[length * (length - 1 - i)]);
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
                return g_key_l;
            }

            return g_key_s;
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
