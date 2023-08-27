namespace Kiseki.Arbiter;

using System.Security.Cryptography;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;

public static class Verifier
{
    private static RSACryptoServiceProvider? rsa;

    public static bool Initialize()
    {
        try
        {
            using TextReader reader = new StringReader(File.ReadAllText(Settings.GetPublicKeyPath()));

            PemReader pem = new(reader);
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pem.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            rsa = new();
            rsa.ImportParameters(rsaParams);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static bool Verify(byte[] data, byte[] signature)
    {
        try
        {
            return rsa!.VerifyData(data, CryptoConfig.MapNameToOID("SHA256")!, signature);
        }
        catch
        {
            return false;
        }
    }
}