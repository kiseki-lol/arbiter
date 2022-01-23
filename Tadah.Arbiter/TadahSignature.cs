using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public static class TadahSignature
    {
        public static bool Verify(string data, string signature)
        {
            try
            {
                byte[] certificateData = Convert.FromBase64String(AppSettings.PublicKey
                    .Replace("-----BEGIN PUBLIC KEY-----", "")
                    .Replace("-----END PUBLIC KEY-----", ""));

                X509Certificate2 certificate = new X509Certificate2(certificateData);
                using (RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)certificate.PublicKey.Key)
                {
                    SHA256Managed sha = new SHA256Managed();

                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);
                    byte[] signatureBytes = Convert.FromBase64String(signature);
                    byte[] hashedData = sha.ComputeHash(dataBytes);

                    bool dataOK = rsa.VerifyData(dataBytes, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);
                    bool signatureOK = rsa.VerifyHash(hashedData, CryptoConfig.MapNameToOID("SHA256"), signatureBytes);

                    return dataOK && signatureOK;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
