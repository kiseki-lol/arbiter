using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Tadah.Arbiter
{
    public static class TadahSignature
    {
        private static RSACryptoServiceProvider publicRsa = ReadPublicKey(AppSettings.PublicKeyPath);

        private static RSACryptoServiceProvider ReadPublicKey(string path)
        {
            using (TextReader reader = new StringReader(File.ReadAllText(path)))
            {
                PemReader pem = new PemReader(reader);
                AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pem.ReadObject();
                RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

                RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
                csp.ImportParameters(rsaParams);
                return csp;
            }
        }

        public static bool Verify(string data, string signature)
        {
            try
            {
                byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                byte[] signatureBytes = Convert.FromBase64String(signature);

                return publicRsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
            }
            catch
            {
                // usually when signature isn't base64
                return false;
            }
        }

        public static bool VerifyData(string data, out string message)
        {
            if (!data.StartsWith("%"))
            {
                message = "";
                return false;
            }

            try
            {
                string signature = data.Substring(1, data.Substring(1).IndexOf("%"));
                string _message = data.Substring(signature.Length + 2);
                string nonce = data.Substring(0, data.IndexOf(":"));

                bool signatureOK = Verify(data, signature);
                bool nonceOK = (Unix.GetTimestamp() - Convert.ToInt32(nonce)) <= 5;
                
                message = _message.Substring(nonce.Length + 1);

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
