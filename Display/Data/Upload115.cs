using Aliyun.OSS;
using Aliyun.OSS.Common;
using Display.Helper;
using Display.Models;
using K4os.Compression.LZ4;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HttpHeaders = Aliyun.OSS.Util.HttpHeaders;

namespace Display.Data
{
    public class Upload115
    {
        private const string Md5Salt = "Qclm8MGWUv59TnrR0XPg";
        private static readonly byte[] CrcSalt = "^j>WD3Kr?J2gLFjD4W2y@"u8.ToArray();
        private static readonly byte[] ServerPubKey = {
            0x04, 0x57, 0xa2, 0x92, 0x57, 0xcd, 0x23, 0x20,
            0xe5, 0xd6, 0xd1, 0x43, 0x32, 0x2f, 0xa4, 0xbb,
            0x8a, 0x3c, 0xf9, 0xd3, 0xcc, 0x62, 0x3e, 0xf5,
            0xed, 0xac, 0x62, 0xb7, 0x67, 0x8a, 0x89, 0xc9,
            0x1a, 0x83, 0xba, 0x80, 0x0d, 0x61, 0x29, 0xf5,
            0x22, 0xd0, 0x34, 0xc8, 0x95, 0xdd, 0x24, 0x65,
            0x24, 0x3a, 0xdd, 0xc2, 0x50, 0x95, 0x3b, 0xee,
            0xba
        };

        private const string UserAgent = Const.UploadUserAgent;
        private const string AppVer = Const.UploadAppVersion;

        private static Upload115 _upload115;

        public Upload115()
        {
            GenerateClientKeyPair();

            // Decrypt
            _aes = Aes.Create();
            _aes.Key = _aesKey;
            _aes.IV = _aesIv;
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.None;

            // client
            Client = GetInfoFromNetwork.CreateClient(new Dictionary<string, string> { { "user-agent", UserAgent } });
            var cookie = AppSettings._115_Cookie;
            //cookie不为空且可用
            if (!string.IsNullOrEmpty(cookie))
            {
                Client.DefaultRequestHeaders.Add("Cookie", cookie);
            }
        }

        public static Upload115 SingleUpload115
        {
            get
            {
                if (_upload115 != null) return _upload115;

                _upload115 = new Upload115();

                return _upload115;
            }
        }

        public HttpClient Client;

        private Aes _aes;
        
        private byte[] _clientPublicKey;
        
        private byte[] _aesKey;


        private byte[] _aesIv;

        private void GenerateClientKeyPair()
        {
            // P-224 曲线
            var curve = SecNamedCurves.GetByName("secp224r1");
            ECDomainParameters ecSpec = new ECDomainParameters(curve.Curve,
                curve.G,
                curve.N,
                curve.H,
                curve.GetSeed());

            // 服务器的公钥
            var serverPoint = curve.Curve.DecodePoint(ServerPubKey);
            ECPublicKeyParameters serverKey = new ECPublicKeyParameters(serverPoint,
                ecSpec);

            ECKeyPairGenerator keyPairGenerator = new ECKeyPairGenerator();
            keyPairGenerator.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP224r1, new SecureRandom()));

            // 计算 客户端私钥 和 服务器公钥 的 SharedSecret
            ECDHBasicAgreement agreement = new ECDHBasicAgreement();

            int length;
            AsymmetricCipherKeyPair keyPair;
            byte[] sharedSecretBytes;
            do
            {
                keyPair = keyPairGenerator.GenerateKeyPair();
                agreement.Init(keyPair.Private);
                BigInteger sharedSecret = agreement.CalculateAgreement(serverKey);
                sharedSecretBytes = sharedSecret.ToByteArray();
                length = sharedSecretBytes.Length;
            } while (length != 28);

            // AES加密的key，取SharedSecret的前16位
            _aesKey = sharedSecretBytes.Take(16).ToArray();

            // AES加密的iv，取SharedSecret的后16位
            _aesIv = sharedSecretBytes.Skip(sharedSecretBytes.Length - 16).Take(16).ToArray();

            // 客户端的公钥
            ECPublicKeyParameters publicKey = (ECPublicKeyParameters)keyPair.Public;

            // 将公钥编码为byte[]，长度29
            var publicKeyBytes = publicKey.Q.GetEncoded(true);

            // 添加一个前缀（29十进制），长度30
            var publicKeyBytes30 = new byte[30];
            publicKeyBytes30[0] = 29;
            publicKeyBytes.CopyTo(publicKeyBytes30, 1);

            _clientPublicKey = publicKeyBytes30;
        }

        private static string GetSign(string userid, string fileid, string target, string userkey)
        {
            string sha1Str = $"{userid}{fileid}{target}0";
            var tmp = HashHelper.ComputeSha1ByContent(sha1Str).ToLower();

            sha1Str = $"{userkey}{tmp}000000";
            sha1Str = HashHelper.ComputeSha1ByContent(sha1Str);

            return sha1Str;
        }

        public async Task UploadTo115(string filePath, string cid, string userId, string userKey)
        {
            var upload115Result = await FastUpload(filePath, cid, userId, userKey);

            // 需要上传
            if (!string.IsNullOrEmpty(upload115Result._object))
            {
                await OssUploadFile(filePath, upload115Result);
            }
        }

        private async Task OssUploadFile(string filePath, Upload115Result upload115Result)
        {
            var httpClient = Client;
            var resp = await httpClient.GetAsync("https://uplb.115.com/3.0/gettoken.php");
            var content = await resp.Content.ReadAsStringAsync();

            var ossToken = JsonConvert.DeserializeObject<OssToken>(content);

            if (ossToken == null) return;

            var endpoint = "http://oss-cn-shenzhen.aliyuncs.com";

            var accessKeySecret = ossToken.AccessKeySecret;
            var accessKeyId = ossToken.AccessKeyId;
            var securityToken = ossToken.SecurityToken;

            // 填写Bucket名称，例如examplebucket。
            var bucketName = upload115Result.bucket;

            // 填写Object完整路径，完整路径中不包含Bucket名称，例如exampledir/exampleobject.txt。
            var objectName = upload115Result._object;

            string callback = upload115Result.callback.callback;
            string callbackVar = upload115Result.callback.callback_var;

            // 创建OSSClient实例。
            var client = new OssClient(endpoint, accessKeyId, accessKeySecret);
            try
            {
                string responseContent;

                var metadata = new ObjectMetadata();
                metadata.AddHeader(HttpHeaders.SecurityToken, securityToken);
                metadata.AddHeader(HttpHeaders.Callback, Convert.ToBase64String(Encoding.Default.GetBytes(callback)));
                metadata.AddHeader(HttpHeaders.CallbackVar, Convert.ToBase64String(Encoding.Default.GetBytes(callbackVar)));

                await using (var fs = File.Open(filePath, FileMode.Open))
                {
                    var putObjectRequest = new PutObjectRequest(bucketName, objectName, fs, metadata);
                    var result = client.PutObject(putObjectRequest);
                    responseContent = GetCallbackResponse(result);
                }

                Debug.WriteLine("Put object:{0} succeeded, callback response content:{1}", objectName, responseContent);
            }
            catch (OssException ex)
            {
                Debug.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
                    ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed with error info: {0}", ex.Message);
            }
        }

        // 读取上传回调返回的消息内容。
        private static string GetCallbackResponse(PutObjectResult putObjectResult)
        {
            string callbackResponse = null;
            using (var stream = putObjectResult.ResponseStream)
            {
                var buffer = new byte[4 * 1024];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                callbackResponse = Encoding.Default.GetString(buffer, 0, bytesRead);
            }
            return callbackResponse;
        }

        public async Task<Upload115Result> FastUpload(string filePath, string cid, string userId, string userKey)
        {
            // target
            string target = "U_1_" + cid;

            var file = new FileInfo(filePath);

            // fileName
            var fileName = file.Name;

            // fileSize
            var fileSize = file.Length;

            // fileId
            var fileId = HashHelper.ComputeSha1ByPath(filePath);

            // signKey，开始为空
            var signKey = "";

            // signVal，开始为空
            var signVal = "";

            // sign
            string sign = GetSign(userId, fileId, target, userKey);

            // user(md5格式)
            var userIdMd5 = HashHelper.ComputeMd5ByContent(userId).ToLower();

            // 机密的内容
            Dictionary<string, string> dataForm = new Dictionary<string, string>()
            {
                {"appid", "0"},
                {"appversion", AppVer},
                {"filename", fileName},
                {"filesize", fileSize.ToString()},
                {"fileid", fileId},
                {"target", target},
                {"userid", userId},
                {"sig", sign},
            };

            Upload115Result upload115Result = null;
            for (var i = 0; i < 3; i++)
            {
                // time
                var timeSpan = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                dataForm["t"] = timeSpan.ToString();

                // token(md5格式)
                var token = $"{Md5Salt}{fileId}{fileSize}{signKey}{signVal}{userId}{timeSpan}{userIdMd5}{AppVer}";
                var tokenMd5 = HashHelper.ComputeMd5ByContent(token).ToLower();
                dataForm["token"] = tokenMd5;

                if (!string.IsNullOrEmpty(signKey) && !string.IsNullOrEmpty(signVal))
                {
                    dataForm["sign_key"] = signKey;
                    dataForm["sign_val"] = signVal;
                }

                upload115Result = await EncodeData(dataForm, timeSpan, _aesKey, _aesIv, _clientPublicKey);

                if (upload115Result == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(upload115Result.sign_key) || string.IsNullOrEmpty(upload115Result.sign_check))
                {
                    return upload115Result;
                };

                signKey = upload115Result.sign_key;
                signVal = HashHelper.ComputeSha1RangeByPath(filePath, upload115Result.sign_check);
            }

            return upload115Result;
        }


        private async Task<Upload115Result> EncodeData(Dictionary<string, string> dataForm, long timeSpan, byte[] aesKey, byte[] aesIv, byte[] publicKeyBytes30)
        {
            var data = string.Join("&", dataForm.Select(x => $"{x.Key}={x.Value}"));
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            // Encrypt
            ICryptoTransform decryption = _aes.CreateEncryptor(_aes.Key, _aes.IV);

            // 补全到BlockSize（16字节，128位）的整数倍
            int blockSize = _aes.BlockSize / 8;
            int paddedLength = blockSize - (dataBytes.Length % blockSize);
            byte[] paddedDataBytes = new byte[dataBytes.Length + paddedLength];

            Array.Copy(dataBytes, paddedDataBytes, dataBytes.Length);

            // 开始加密
            var encryptedDataBytes = decryption.TransformFinalBlock(paddedDataBytes, 0, paddedDataBytes.Length);

            var kEc = EncryptToken(publicKeyBytes30, timeSpan);

            var url = $"https://uplb.115.com/4.0/initupload.php?k_ec={kEc}";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = new ByteArrayContent(encryptedDataBytes);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var httpClient = Client;
            var response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode) return null;

            var contentBytes = await response.Content.ReadAsByteArrayAsync();

            return ReceiveData(contentBytes, aesKey: aesKey, aesIv: aesIv);

        }

        private Upload115Result ReceiveData(byte[] data, byte[] aesKey, byte[] aesIv)
        {
            byte[] cipherText = new byte[data.Length - 12];

            Array.Copy(data, cipherText, cipherText.Length);

            byte[] tail = new byte[12];
            Array.Copy(data, data.Length - 12, tail, 0, tail.Length);

            uint crc32 = Crc32Helper.Compute(CrcSalt.Concat(tail.Take(8)).ToArray());
            var trueCrc32 = BitConverter.ToUInt32(tail.Skip(8).Take(4).ToArray(), 0);

            if (crc32 != trueCrc32)
            {
                return null;
            }

            ICryptoTransform decryption = _aes.CreateDecryptor(_aes.Key, _aes.IV);

            var plaintext = decryption.TransformFinalBlock(cipherText, 0, cipherText.Length);

            // Decompress
            for (int i = 0; i < 4; i++)
                tail[i] ^= tail[7];

            // 解密后的大小
            int dstSize = BitConverter.ToInt32(tail.Take(4).ToArray(), 0);

            // 解密前的大小
            int srcSize = BitConverter.ToUInt16(plaintext.Take(2).ToArray(), 0);

            if (srcSize > plaintext.Length)
            {
                return null;
            }

            // 需要解密的信息
            byte[] compressedData = plaintext.Skip(2).Take(srcSize).ToArray();

            var target = new byte[dstSize]; // or source.Length * 255 to be safe

            // 解密
            var decodedCount = LZ4Codec.Decode(
                compressedData,
                target);

            if(decodedCount == -1) return null;

            var content = Encoding.Default.GetString(target);
            var upload115Result = JsonConvert.DeserializeObject<Upload115Result>(content);

            return upload115Result;
        }


        static string EncryptToken(byte[] publicKeyBytes30, long timestamp)
        {
            byte[] time = BitConverter.GetBytes((uint)timestamp);

            // 两个随机数
            Random random = new Random();
            var r1 = (byte)random.Next(256);
            var r2 = (byte)random.Next(256);

            byte[] tmp = new byte[44];

            // 0~14
            for (var i = 0; i < 15; i++)
            {
                tmp[i] = (byte)(publicKeyBytes30[i] ^ r1);
            }

            // 15
            tmp[15] = r1;

            // 16
            tmp[16] = (byte)(115 ^ r1);

            // 17~19
            for (var i = 17; i < 20; i++)
            {
                tmp[i] = r1;
            }

            // 20~23
            for (int i = 20; i < 24; i++)
            {
                tmp[i] = (byte)(r1 ^ time[23 - i]);
            }

            // 24~38
            for (int i = 15; i < publicKeyBytes30.Length; i++)
            {
                tmp[i + 9] = (byte)(publicKeyBytes30[i] ^ r2);
            }

            // 39
            tmp[39] = r2;

            // 40
            tmp[40] = (byte)(1 ^ r1);

            // 41~43
            for (var i = 41; i < 44; i++)
            {
                tmp[i] = r2;
            }

            var crc32Bytes = new byte[65];
            CrcSalt.CopyTo(crc32Bytes, 0);
            Array.Copy(tmp, 0, crc32Bytes, CrcSalt.Length, 44);

            // 结果是python的binascii.crc32结果的倒序，不清楚原因
            var bytes = CalcCrc32(crc32Bytes);

            var token = new byte[48];
            tmp.CopyTo(token, 0);

            // 44~48
            bytes.CopyTo(token, 44);

            var s = Convert.ToBase64String(token);

            return s;
        }

        private static byte[] CalcCrc32(byte[] bytes)
        {
            var newCrc32 = Crc32Helper.Compute(bytes);
            var newResult = BitConverter.GetBytes(newCrc32);

            return newResult;
        }
    }
}
