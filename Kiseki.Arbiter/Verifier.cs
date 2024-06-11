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
        const string LOG_IDENT = "Verifier::Initialize";

        try
        {
            using TextReader reader = new StringReader(File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Settings.GetPublicKeyPath()));

            PemReader pem = new(reader);
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pem.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            rsa = new();
            rsa.ImportParameters(rsaParams);
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed: {ex}", LogSeverity.Debug);

            return false;
        }

        Logger.Write(LOG_IDENT, "OK!", LogSeverity.Debug);

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