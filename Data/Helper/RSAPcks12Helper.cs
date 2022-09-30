using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Helper
{
    public class RSAPcks12Helper
    {
        #region 私钥签名公钥验签
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="p12Cert">P12/PFX证书的 base64格式</param>
        /// <param name="certPwd">证书密码</param>
        /// <param name="contentData">要签名的数据</param>
        /// <param name="signAlgorithm">签名算法</param>
        /// <returns></returns>
        public static string SignDataWithPcks12(string p12Cert, string certPwd, byte[] contentData, string signAlgorithm = "SHA1WithRSA")
        {
            return Convert.ToBase64String(SignDataWithPcks12(Convert.FromBase64String(p12Cert), certPwd.ToCharArray(), contentData, signAlgorithm));
        }
        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="p12CertData">P12/PFX证书</param>
        /// <param name="certPwdData">证书密码</param>
        /// <param name="contentData">要签名的数据</param>
        /// <param name="signAlgorithm">签名算法</param>
        /// <returns></returns>
        public static byte[] SignDataWithPcks12(byte[] p12CertData, char[] certPwdData, byte[] contentData, string signAlgorithm = "SHA1WithRSA")
        {
            var key = GetFromPcks12(p12CertData, certPwdData, (store, keyAlias) => store.GetKey(keyAlias).Key);
            ISigner oSig = SignerUtilities.GetSigner(signAlgorithm);
            oSig.Init(true, key);
            oSig.BlockUpdate(contentData, 0, contentData.Length);
            return oSig.GenerateSignature();
        }
        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="x509Cert">x509通用证书base64格式</param>
        /// <param name="content">原始数据</param>
        /// <param name="sign">签名结果base64格式</param>
        /// <param name="signAlgorithm">签名算法</param>
        /// <returns></returns>
        public static bool VerifyDataWithX509(string x509Cert, byte[] content, string sign, string signAlgorithm = "SHA1WithRSA")
        {
            return VerifyDataWithX509(Convert.FromBase64String(x509Cert), content, Convert.FromBase64String(sign), signAlgorithm);
        }
        /// <summary>
        /// 验签
        /// </summary>
        /// <param name="x509CertData">x509通用证书</param>
        /// <param name="contentData">原始数据</param>
        /// <param name="signData">签名结果</param>
        /// <param name="signAlgorithm">签名算法</param>
        /// <returns></returns>
        public static bool VerifyDataWithX509(byte[] x509CertData, byte[] contentData, byte[] signData, string signAlgorithm = "SHA1WithRSA")
        {
            var publicKey = GetAsymmetricKeyParameterWithX509(x509CertData);
            return VerifyData(publicKey, contentData, signData, signAlgorithm);
        }
        private static T GetFromPcks12<T>(byte[] p12CertData, char[] certPwdData, Func<Pkcs12Store, string, T> func)
        {
            using (MemoryStream stream = new MemoryStream(p12CertData))
            {
                Pkcs12Store store = new Pkcs12Store(stream, certPwdData);
                string keyAlias = null;
                foreach (string alias in store.Aliases)
                {
                    try
                    {
                        keyAlias = alias;
                        return func(store, keyAlias);
                    }
                    catch { }
                }
                throw new ArgumentException();
            }
        }
        private static bool VerifyData(AsymmetricKeyParameter publicKey, byte[] contentData, byte[] signData, string signAlgorithm = "SHA1WithRSA")
        {
            ISigner oSig = SignerUtilities.GetSigner(signAlgorithm);
            oSig.Init(false, publicKey);
            oSig.BlockUpdate(contentData, 0, contentData.Length);
            return oSig.VerifySignature(signData);
        }
        #endregion

        #region 公钥、私钥获取 pcks8格式
        /// <summary>
        /// 获取RSA私钥
        /// </summary>
        /// <param name="p12Cert">P12/PFX证书的 base64格式</param>
        /// <param name="certPwd">证书密码</param>
        /// <returns></returns>
        public static string GetPrivateKeyFromPcks12(string p12Cert, string certPwd)
        {
            return Convert.ToBase64String(GetPrivateKeyFromPcks12(Convert.FromBase64String(p12Cert), certPwd.ToCharArray()));
        }
        /// <summary>
        /// 获取RSA私钥
        /// </summary>
        /// <param name="p12CertData">P12/PFX证书</param>
        /// <param name="certPwdData">证书密码</param>
        /// <returns></returns>
        public static byte[] GetPrivateKeyFromPcks12(byte[] p12CertData, char[] certPwdData)
        {
            var privateKeyParam = GetFromPcks12(p12CertData, certPwdData, (store, keyAlias) => store.GetKey(keyAlias).Key);
            PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParam);
            return privateKeyInfo.ToAsn1Object().GetEncoded();
        }
        /// <summary>
        /// 获取RSA公钥
        /// </summary>
        /// <param name="p12Cert">P12/PFX证书的 base64格式</param>
        /// <param name="certPwd">证书密码</param>
        /// <returns></returns>
        public static string GetPublicKeyFromPcks12(string p12Cert, string certPwd)
        {
            return Convert.ToBase64String(GetPublicKeyFromPcks12(Convert.FromBase64String(p12Cert), certPwd.ToCharArray()));
        }
        /// <summary>
        /// 获取RSA公钥
        /// </summary>
        /// <param name="p12CertData">P12/PFX证书</param>
        /// <param name="certPwdData">证书密码</param>
        /// <returns></returns>
        public static byte[] GetPublicKeyFromPcks12(byte[] p12CertData, char[] certPwdData)
        {
            X509CertificateEntry[] chain = GetFromPcks12(p12CertData, certPwdData,
                (store, keyAlias) => store.GetCertificateChain(keyAlias));
            var cert = chain[0].Certificate;
            var publicKey = cert.GetPublicKey();
            return SerializePublicKey(publicKey);
        }
        /// <summary>
        /// 获取RSA公钥
        /// </summary>
        /// <param name="x509Cert">x509通用证书base64格式</param>
        /// <returns></returns>
        public static string GetPublicKeyFromX509(string x509Cert)
        {
            return Convert.ToBase64String(GetPublicKeyFromX509(Convert.FromBase64String(x509Cert)));
        }
        /// <summary>
        /// 获取RSA公钥
        /// </summary>
        /// <param name="x509CertData">x509通用证书</param>
        /// <returns></returns>
        public static byte[] GetPublicKeyFromX509(byte[] x509CertData)
        {
            var publicKey = GetAsymmetricKeyParameterWithX509(x509CertData);
            return SerializePublicKey(publicKey);
        }
        private static byte[] SerializePublicKey(AsymmetricKeyParameter publicKey)
        {
            SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
            return SerializeKey(publicKeyInfo);
        }
        private static byte[] SerializeKey(Asn1Encodable asn1)
        {
            return asn1.ToAsn1Object().GetDerEncoded();
        }
        private static AsymmetricKeyParameter GetAsymmetricKeyParameterWithX509(byte[] x509CertData)
        {
            var certificate = new X509CertificateParser().ReadCertificate(x509CertData);
            return certificate.GetPublicKey();
        }
        #endregion

        #region 公钥加密私钥解密
        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="x509Cert">x509通用证书base64格式</param>
        /// <param name="contentData">待加密的数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static string EncryptWithX509(string x509Cert, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            return Convert.ToBase64String(EncryptWithX509(Convert.FromBase64String(x509Cert), contentData, algorithm));
        }
        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="x509CertData">x509通用证书</param>
        /// <param name="contentData">待加密的数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static byte[] EncryptWithX509(byte[] x509CertData, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            return EncryptWithPublicKey(GetPublicKeyFromX509(x509CertData), contentData, algorithm);
        }
        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="publicKey">RSA公钥 base64格式</param>
        /// <param name="contentData">待加密的数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static string EncryptWithPublicKey(string publicKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            return Convert.ToBase64String(EncryptWithPublicKey(Convert.FromBase64String(publicKey), contentData, algorithm));
        }
        /// <summary>
        /// 公钥加密
        /// </summary>
        /// <param name="publicKey">RSA公钥</param>
        /// <param name="contentData">待加密的数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static byte[] EncryptWithPublicKey(byte[] publicKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(publicKey);
            return Transform(publicKeyParam, contentData, algorithm, true);
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="p12Cert">P12/PFX证书的 base64格式</param>
        /// <param name="certPwd">证书密码</param>
        /// <param name="content">待解密数据 base64格式</param>
        /// <param name="encoding">解密出来的数据编码格式，默认UTF-8</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static string DecryptWithPcks12(string p12Cert, string certPwd, string content, string encoding = "UTF-8", string algorithm = "RSA/ECB/PKCS1Padding")
        {
            return Encoding.GetEncoding(encoding).GetString(DecryptWithPcks12(Convert.FromBase64String(p12Cert), certPwd.ToCharArray(), Convert.FromBase64String(content), algorithm));
        }
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="p12Data">P12/PFX证书</param>
        /// <param name="certPwdData">证书密码</param>
        /// <param name="contentData">待解密数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static byte[] DecryptWithPcks12(byte[] p12Data, char[] certPwdData, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            var privateKey = GetFromPcks12(p12Data, certPwdData, (store, keyAlias) => store.GetKey(keyAlias).Key);
            return Transform(privateKey, contentData, algorithm, false);
        }
        /// <summary>
        /// 私钥解密
        /// </summary>
        /// <param name="privateKey">RSA私钥  base64格式</param>
        /// <param name="content">待解密数据 base64格式</param>
        /// <param name="encoding">解密出来的数据编码格式，默认UTF-8</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static string DecryptWithPrivateKey(string privateKey, string content, string encoding = "UTF-8", string algorithm = "RSA/ECB/PKCS1Padding")
        {
            return Encoding.GetEncoding(encoding).GetString(DecryptWithPrivateKey(Convert.FromBase64String(privateKey), Convert.FromBase64String(content), algorithm));
        }
        /// <summary>
        /// 私钥解密
        /// </summary>
        /// <param name="privateKey">RSA私钥</param>
        /// <param name="contentData">待解密数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static byte[] DecryptWithPrivateKey(byte[] privateKey, byte[] contentData, string algorithm)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(privateKey);
            return Transform(privateKeyParam, contentData, algorithm, false);
        }
        private static byte[] Transform(AsymmetricKeyParameter key, byte[] contentData, string algorithm, bool forEncryption)
        {
            var c = CipherUtilities.GetCipher(algorithm);
            c.Init(forEncryption, new ParametersWithRandom(key));
            return c.DoFinal(contentData);
        }

        #endregion

        #region 私钥加密，公钥解密
        /// <summary>
        /// 私钥加密
        /// </summary>
        /// <param name="privateKey">RSA私钥 base64格式</param>
        /// <param name="contentData">待加密的数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static string EncryptWithPrivateKey(string privateKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            return Convert.ToBase64String(EncryptWithPrivateKey(Convert.FromBase64String(privateKey), contentData, algorithm));
        }
        /// <summary>
        /// 私钥加密
        /// </summary>
        /// <param name="privateKey">RSA私钥</param>
        /// <param name="contentData">待加密的数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static byte[] EncryptWithPrivateKey(byte[] privateKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(privateKey);
            return Transform(privateKeyParam, contentData, algorithm, true);
        }
        /// <summary>
        /// 公钥解密
        /// </summary>
        /// <param name="publicKey">RSA公钥  base64格式</param>
        /// <param name="content">待解密数据 base64格式</param>
        /// <param name="encoding">解密出来的数据编码格式，默认UTF-8</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static string DecryptWithPublicKey(string publicKey, byte[] content, string encoding = "UTF-8", string algorithm = "RSA/ECB/PKCS1Padding")
        {
            //return DecryptWithPublicKey(Convert.FromBase64String(publicKey), content, algorithm);
            return Encoding.GetEncoding(encoding).GetString(DecryptWithPublicKey(Convert.FromBase64String(publicKey), content, algorithm));
        }
        /// <summary>
        /// 公钥解密
        /// </summary>
        /// <param name="publicKey">RSA公钥</param>
        /// <param name="contentData">待解密数据</param>
        /// <param name="algorithm">加密算法</param>
        /// <returns></returns>
        public static byte[] DecryptWithPublicKey(byte[] publicKey, byte[] contentData, string algorithm = "RSA/ECB/PKCS1Padding")
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(publicKey);
            return Transform(publicKeyParam, contentData, algorithm, false);
        }
        #endregion

        #region 生成pfx
        /// <summary>
        /// 根据crt以及RSA私钥生成pfx证书
        /// </summary>
        /// <param name="passWord">证书密码</param>
        /// <param name="x509CertData">crt证书</param>
        /// <param name="privateKey">Rsa私钥</param>
        /// <param name="x509CertChainData">证书链</param>
        /// <param name="alias">默认别名</param>
        /// <returns></returns>
        public static MemoryStream GeneratePFX(string passWord, byte[] x509CertData, byte[] privateKey, byte[] x509CertChainData = null, string alias = "PrimaryCertificate")
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(privateKey);
            var certEntry = GetX509CertificateEntry(x509CertData);
            Pkcs12Store store = new Pkcs12StoreBuilder().Build();
            store.SetCertificateEntry(alias, certEntry);
            X509CertificateEntry[] chain = new X509CertificateEntry[1];
            if (x509CertChainData != null)
            {
                chain = new X509CertificateEntry[2];
                chain[1] = GetX509CertificateEntry(x509CertChainData);
            }
            chain[0] = certEntry;
            store.SetKeyEntry(alias, new AsymmetricKeyEntry(privateKeyParam), chain);   //设置私钥  
            var ms = new MemoryStream();
            store.Save(ms, passWord.ToCharArray(), new SecureRandom());
            ms.Position = 0;
            return ms;
        }
        private static X509CertificateEntry GetX509CertificateEntry(byte[] certData)
        {
            var certificate = new X509CertificateParser().ReadCertificate(certData);
            X509CertificateEntry certEntry = new X509CertificateEntry(certificate);
            return certEntry;
        }
        #endregion
    }

}
