using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server.Algorithm
{
    public class Aes256
    {
        private const int KeyLength = 32;
        private const int AuthKeyLength = 64;
        private const int IvLength = 16;
        private const int HmacSha256Length = 32;
        private readonly byte[] _key;
        private readonly byte[] _authKey;

        private static readonly byte[] Salt =
        {
            0xBF, 0xEB, 0x1E, 0x56, 0xFB, 0xCD, 0x97, 0x3B, 0xB2, 0x19, 0x2, 0x24, 0x30, 0xA5, 0x78, 0x43, 0x0, 0x3D, 0x56,
            0x44, 0xD2, 0x1E, 0x62, 0xB9, 0xD4, 0xF1, 0x80, 0xE7, 0xE6, 0xC3, 0x39, 0x41
        };

        public Aes256(string masterKey)
        {
            if (string.IsNullOrEmpty(masterKey))
                throw new ArgumentException($"{nameof(masterKey)} can not be null or empty.");

            using (Rfc2898DeriveBytes derive = new Rfc2898DeriveBytes(masterKey, Salt, 50000))
            {
                _key = derive.GetBytes(KeyLength);
                _authKey = derive.GetBytes(AuthKeyLength);
            }
        }

        public string Encrypt(string input)
        {
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(input)));
        }
        public byte[] Encrypt(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException($"{nameof(input)} can not be null.");

            using (var ms = new MemoryStream())
            {
                ms.Position = HmacSha256Length;
                using (var aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.KeySize = 256;
                    aesProvider.BlockSize = 128;
                    aesProvider.Mode = CipherMode.CBC;
                    aesProvider.Padding = PaddingMode.PKCS7;
                    aesProvider.Key = _key;
                    aesProvider.GenerateIV();

                    using (var cs = new CryptoStream(ms, aesProvider.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        ms.Write(aesProvider.IV, 0, aesProvider.IV.Length);
                        cs.Write(input, 0, input.Length);
                        cs.FlushFinalBlock();

                        using (var hmac = new HMACSHA256(_authKey))
                        {
                            byte[] hash = hmac.ComputeHash(ms.ToArray(), HmacSha256Length, ms.ToArray().Length - HmacSha256Length);
                            ms.Position = 0;
                            ms.Write(hash, 0, hash.Length);
                        }
                    }
                }

                return ms.ToArray();
            }
        }

        public string Decrypt(string input)
        {
            return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(input)));
        }

        public byte[] Decrypt(byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException($"{nameof(input)} can not be null.");

            using (var ms = new MemoryStream(input))
            {
                using (var aesProvider = new AesCryptoServiceProvider())
                {
                    aesProvider.KeySize = 256;
                    aesProvider.BlockSize = 128;
                    aesProvider.Mode = CipherMode.CBC;
                    aesProvider.Padding = PaddingMode.PKCS7;
                    aesProvider.Key = _key;

                    using (var hmac = new HMACSHA256(_authKey))
                    {
                        var hash = hmac.ComputeHash(ms.ToArray(), HmacSha256Length, ms.ToArray().Length - HmacSha256Length);
                        byte[] receivedHash = new byte[HmacSha256Length];
                        ms.Read(receivedHash, 0, receivedHash.Length);

                        if (!AreEqual(hash, receivedHash))
                            throw new CryptographicException("Invalid message authentication code (MAC).");
                    }

                    byte[] iv = new byte[IvLength];
                    ms.Read(iv, 0, IvLength);
                    aesProvider.IV = iv;

                    using (var cs = new CryptoStream(ms, aesProvider.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        byte[] temp = new byte[ms.Length - IvLength + 1];
                        byte[] data = new byte[cs.Read(temp, 0, temp.Length)];
                        Buffer.BlockCopy(temp, 0, data, 0, data.Length);
                        return data;
                    }
                }
            }
        }
        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private bool AreEqual(byte[] a1, byte[] a2)
        {
            bool result = true;
            for (int i = 0; i < a1.Length; ++i)
            {
                if (a1[i] != a2[i])
                    result = false;
            }
            return result;
        }
    }
}