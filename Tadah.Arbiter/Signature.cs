using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Tadah
{
    public static class Signature
    {
        private static readonly RSACryptoServiceProvider rsa = ReadPublicKey(Arbiter.Configuration.AppSettings["PublicKeyPath"]);

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
                return rsa.VerifyData(data, CryptoConfig.MapNameToOID("SHA256"), signature);
            }
            catch
            {
                return false;
            }
        }

        // This is for rudimentary signed data in the format of "%signature%nonce;message"
        // Currently just used for TampaJob::ExecuteScript
        public static bool VerifyData(string data, out string message)
        {
            if (!data.StartsWith("%"))
            {
                message = "";
                return false;
            }

            try
            {
                string signature = data.Substring(1, data[1..].IndexOf("%"));
                string nonce = data[(signature.Length + 2)..data.IndexOf(":")];

                message = data[(signature.Length + nonce.Length + 3)..];

                bool signatureOK = Verify(Encoding.UTF8.GetBytes(message), Encoding.UTF8.GetBytes(signature));
                bool nonceOK = (Unix.Now() - int.Parse(nonce)) <= 5;

                return nonceOK && signatureOK;
            }
            catch
            {
                message = "";
                return false;
            }
        }
    }
}
