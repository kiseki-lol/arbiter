using System.Security.Cryptography;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Kiseki.Arbiter
{
    public static class Signature
    {
        private static readonly RSACryptoServiceProvider rsa = ReadPublicKey(Settings.GetPublicKeyPath());

        private static RSACryptoServiceProvider ReadPublicKey(string path)
        {
            using TextReader reader = new StringReader(File.ReadAllText(path));

            PemReader pem = new(reader);
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pem.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            RSACryptoServiceProvider csp = new();
            csp.ImportParameters(rsaParams);
            return csp;
        }

        public static bool Verify(byte[] data, byte[] signature)
        {
            try
            {
                return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256")!, signature);
            }
            catch
            {
                return false;
            }
        }
    }
}