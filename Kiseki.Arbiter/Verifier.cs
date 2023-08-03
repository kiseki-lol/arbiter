using System.Security.Cryptography;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Kiseki.Arbiter;

public static class Verifier
{
    private static RSACryptoServiceProvider? rsa;

    public static void Initialize()
    {
        using TextReader reader = new StringReader(File.ReadAllText(Settings.GetPublicKeyPath()));

        PemReader pem = new(reader);
        AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pem.ReadObject();
        RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

        rsa = new();
        rsa.ImportParameters(rsaParams);
    }

    public static bool Verify(byte[] data, byte[] signature)
    {
        try
        {
            return rsa?.VerifyData(data, CryptoConfig.MapNameToOID("SHA256")!, signature) ?? false;
        }
        catch
        {
            return false;
        }
    }
}