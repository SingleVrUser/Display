using Display.Helper.Crypto;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
namespace Display.Services.Upload;

internal class UploadKey
{
    private static UploadKey _uploadKey;

    public static UploadKey Instance
    {
        get
        {
            _uploadKey ??= new UploadKey();

            return _uploadKey;
        }
    }

    public byte[] AesKey;
    public byte[] AesIv;
    public byte[] ClientPublicKey;

    private void GenerateClientKeyPair()
    {
        // P-224 曲线
        var curve = SecNamedCurves.GetByName("secp224r1");
        var ecSpec = new ECDomainParameters(curve.Curve,
            curve.G,
            curve.N,
            curve.H,
            curve.GetSeed());

        // 服务器的公钥
        var serverPoint = curve.Curve.DecodePoint(UploadEncryptHelper.ServerPubKey);
        var serverKey = new ECPublicKeyParameters(serverPoint,
            ecSpec);

        var keyPairGenerator = new ECKeyPairGenerator();
        keyPairGenerator.Init(new ECKeyGenerationParameters(SecObjectIdentifiers.SecP224r1, new SecureRandom()));

        // 计算 客户端私钥 和 服务器公钥 的 SharedSecret
        var agreement = new ECDHBasicAgreement();

        int length;
        AsymmetricCipherKeyPair keyPair;
        byte[] sharedSecretBytes;
        do
        {
            keyPair = keyPairGenerator.GenerateKeyPair();
            agreement.Init(keyPair.Private);
            var sharedSecret = agreement.CalculateAgreement(serverKey);
            sharedSecretBytes = sharedSecret.ToByteArray();
            length = sharedSecretBytes.Length;
        } while (length != 28);

        // AES加密的key，取SharedSecret的前16位
        AesKey = sharedSecretBytes.Take(16).ToArray();

        // AES加密的iv，取SharedSecret的后16位
        AesIv = sharedSecretBytes.Skip(sharedSecretBytes.Length - 16).Take(16).ToArray();

        // 客户端的公钥
        var publicKey = (ECPublicKeyParameters)keyPair.Public;

        // 将公钥编码为byte[]，长度29
        var publicKeyBytes = publicKey.Q.GetEncoded(true);

        // 添加一个前缀（29十进制），长度30
        var publicKeyBytes30 = new byte[30];
        publicKeyBytes30[0] = 29;
        publicKeyBytes.CopyTo(publicKeyBytes30, 1);

        ClientPublicKey = publicKeyBytes30;
    }

    public UploadKey()
    {
        GenerateClientKeyPair();
    }


}