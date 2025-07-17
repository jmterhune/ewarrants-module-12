using DotNetNuke.Services.Exceptions;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace tjc.Modules.eWarrants.Components
{
    public class Encryptor
    {
        private readonly string TamperProofKey = GetKey("Warrants");

        public string QueryStringEncode(string value)
        {
            return HttpUtility.UrlEncode(TamperProofStringEncode(value, TamperProofKey));
        }

        public string QueryStringEncode(string value, string key)
        {
            return HttpUtility.UrlEncode(TamperProofStringEncode(value, key));
        }

        public string StringEncode(string value, string key)
        {
            return TamperProofStringEncode(value, key);
        }

        public string QueryStringDecode(string value)
        {
            return TamperProofStringDecode(value, TamperProofKey);
        }

        public string QueryStringDecode(string value, string key)
        {
            return TamperProofStringDecode(value, key);
        }

        private string TamperProofStringEncode(string value, string key)
        {
            System.Security.Cryptography.MACTripleDES mac3des = new System.Security.Cryptography.MACTripleDES();
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value)) + '-' + Convert.ToBase64String(mac3des.ComputeHash(System.Text.Encoding.UTF8.GetBytes(value)));
        }

        private string TamperProofStringDecode(string value, string key)
        {
            System.Security.Cryptography.MACTripleDES mac3des = new System.Security.Cryptography.MACTripleDES();
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            mac3des.Key = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(key));

            string dataValue;
            try
            {
                dataValue = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value.Split('-')[0]));
                string storedHash = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value.Split('-')[1]));
                string calcHash = System.Text.Encoding.UTF8.GetString(mac3des.ComputeHash(System.Text.Encoding.UTF8.GetBytes(dataValue)));

                if (storedHash != calcHash)
                    // Data was corrupted

                    throw new ArgumentException("Hash value does not match");
            }
            catch (Exception)
            {
                throw new ArgumentException("Invalid TamperProofString");
            }

            return dataValue;
        }

        private static string GetKey(string key)
        {
            return key.Remove(0, key.Length / 2);
        }

        public byte[] EncryptStream(byte[] input)
        {
            RijndaelManaged rijn = new RijndaelManaged();
            byte[] encrypted;
            byte[] key = new byte[] { 0x22, 0xC0, 0x6D, 0xCB, 0x23, 0xA6, 0x3, 0x1B, 0x5A, 0x1D, 0xD3, 0x9F, 0x85, 0xD, 0xC1, 0x72, 0xED, 0xF4, 0x54, 0xE6, 0xBA, 0x65, 0xC, 0x22, 0x62, 0xBE, 0xF3, 0xE, 0x14, 0x81, 0xA8, 0xA };
            // 32
            byte[] IV = new byte[] { 0x43, 0xB1, 0x93, 0xB, 0x1A, 0x87, 0x52, 0x62, 0xFB, 0x8, 0xD, 0xC0, 0xCA, 0x40, 0xC2, 0xDB };
            // 16
            // Get an encryptor.
            ICryptoTransform encryptor = rijn.CreateEncryptor(key, IV);

            try
            {
                // Encrypt the data.
                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);


                // Write all data to the crypto stream and flush it.
                csEncrypt.Write(input, 0, input.Length);
                csEncrypt.FlushFinalBlock();

                // Get encrypted array of bytes.
                encrypted = msEncrypt.ToArray();
            }
            catch (Exception exc)
            {
                encrypted = null;
                Exceptions.LogException(exc);
            }

            return encrypted;
        }

        public byte[] DecryptStream(byte[] input)
        {
            RijndaelManaged rijn = new RijndaelManaged();
            byte[] decrypted;
            byte[] key = new byte[] { 0x22, 0xC0, 0x6D, 0xCB, 0x23, 0xA6, 0x3, 0x1B, 0x5A, 0x1D, 0xD3, 0x9F, 0x85, 0xD, 0xC1, 0x72, 0xED, 0xF4, 0x54, 0xE6, 0xBA, 0x65, 0xC, 0x22, 0x62, 0xBE, 0xF3, 0xE, 0x14, 0x81, 0xA8, 0xA };
            // 32
            byte[] IV = new byte[] { 0x43, 0xB1, 0x93, 0xB, 0x1A, 0x87, 0x52, 0x62, 0xFB, 0x8, 0xD, 0xC0, 0xCA, 0x40, 0xC2, 0xDB };
            // 16 


            // Get a decryptor that uses the same key and IV as the encryptor.
            ICryptoTransform decryptor = rijn.CreateDecryptor(key, IV);

            // Now decrypt the previously encrypted message using the decryptor
            // obtained in the above step.
            MemoryStream msDecrypt = new MemoryStream(input);
            CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);

            decrypted = new byte[input.Length - 1 + 1];

            // Read the data out of the crypto stream.
            csDecrypt.Read(decrypted, 0, decrypted.Length);

            return decrypted;
        }
    }
}