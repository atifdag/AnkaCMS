﻿using AnkaCMS.Core.Security;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AnkaCMS.Core.Helpers
{

    /// <summary>
    /// Şifreleme işlemleri için yardımcı sınıf
    /// </summary>
    public static class SecurityHelper
    {
        private const string InitVector = "d4464086+f8f7!42";
        private const int KeySize = 256;
        private const int PasswordIterations = 10000;
        private const string SaltValue = "c077cff4-72bd-4636-afb9-d3148b515bbd";
        private const string PassPhrase = "c9c806da-0c16-4e73-b78d-d78dd8c382d5";


        /// <summary>
        /// Düz metni kriptolu hale getirir.
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string Encrypt(this string plainText)
        {
            var initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
            var passwordBytes = Encoding.UTF8.GetBytes(PassPhrase);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            var saltValueBytes = Encoding.UTF8.GetBytes(SaltValue);
            using var password = new Rfc2898DeriveBytes(passwordBytes, saltValueBytes, PasswordIterations);
            var keyBytes = password.GetBytes(KeySize / 8);
            using var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC };
            using var encryptor = rijndaelManaged.CreateEncryptor(keyBytes, initVectorBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            var t = memoryStream.ToArray();
            cryptoStream.Close();
            memoryStream.Close();
            return Convert.ToBase64String(t);
        }

        /// <summary>
        /// Kriptolu bir ifadeyi düz metin haline getirir.
        /// </summary>
        /// <param name="encryptedText"></param>
        /// <returns></returns>
        public static string Decrypt(this string encryptedText)
        {
            var encryptedTextBytes = Convert.FromBase64String(encryptedText);
            var initVectorBytes = Encoding.UTF8.GetBytes(InitVector);
            var passwordBytes = Encoding.UTF8.GetBytes(PassPhrase);
            var saltValueBytes = Encoding.UTF8.GetBytes(SaltValue);
            using var password = new Rfc2898DeriveBytes(passwordBytes, saltValueBytes, PasswordIterations);
            var keyBytes = password.GetBytes(KeySize / 8);
            using var rijndaelManaged = new RijndaelManaged { Mode = CipherMode.CBC };
            using var decryptor = rijndaelManaged.CreateDecryptor(keyBytes, initVectorBytes);
            using var memoryStream = new MemoryStream(encryptedTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            var plainTextBytes = new byte[encryptedTextBytes.Length];
            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
        }

        /// <summary>
        /// HMACSHA256 algoritması kullanarak özet oluşturur.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ToHmacSha256(string input, string key)
        {
            var byteKey = Encoding.UTF8.GetBytes(key);
            using var hash = new HMACSHA256(byteKey);
            var bytes = hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identity"></param>
        /// <param name="key"></param>
        /// <returns></returns>

        public static string GetJwt(AnkaCMSIdentity identity, string key)
        {
            const string header = "{'alg': 'HS256','typ': 'JWT'}";

            var serializedObject = JsonConvert.SerializeObject(identity, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            var o = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(serializedObject);
            o.Property("Claims").Remove();

            var firstSection = Convert.ToBase64String(Encoding.UTF8.GetBytes(header)) + "." + Convert.ToBase64String(Encoding.UTF8.GetBytes(o.ToString()));
            var signature = ToHmacSha256(firstSection, key);
            var jwt = firstSection + "." + signature;
            return jwt;
        }

        /// <summary>
        /// Kurallara uygun şifre olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="password"></param>
        /// <param name="minLength"></param>
        /// <param name="numUpper"></param>
        /// <param name="numLower"></param>
        /// <param name="numNumbers"></param>
        /// <param name="numSpecial"></param>
        /// <returns></returns>
        public static bool ValidatePassword(this string password, int minLength = 8, int numUpper = 1, int numLower = 1, int numNumbers = 1, int numSpecial = 1)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            var upper = new System.Text.RegularExpressions.Regex("[A-Z]");
            var lower = new System.Text.RegularExpressions.Regex("[a-z]");
            var number = new System.Text.RegularExpressions.Regex("[0-9]");
            var special = new System.Text.RegularExpressions.Regex("[^a-zA-Z0-9]");

            if (password.Length < minLength)
            {
                return false;
            }

            if (upper.Matches(password).Count < numUpper)
            {
                return false;
            }
            if (lower.Matches(password).Count < numLower)
            {
                return false;
            }
            if (number.Matches(password).Count < numNumbers)
            {
                return false;
            }
            return special.Matches(password).Count >= numSpecial;
        }

        /// <summary>
        /// SHA512 algoritması kullanarak özet oluşturur. 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSha512(this string str)
        {
            using var hashTool = SHA512.Create();
            var encryptedBytes = hashTool.ComputeHash(Encoding.UTF8.GetBytes(str));
            var stringBuilder = new StringBuilder();
            foreach (var t in encryptedBytes)
            {
                stringBuilder.Append(t.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Kullanıcılar için rastgele şifre oluşturur.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>

        public static string CreatePassword(int length)
        {
            const string upperCaseChars = "ABCDEFGHJKLMNPQRSTWXYZ";
            const string lowerCaseChars = "abcdefgijkmnopqrstwxyz";
            const string numericChars = "0123456789";
            const string specialChars = "%#+-!@?";
            var charGroups = new[]
            {
                upperCaseChars.ToCharArray(),
                lowerCaseChars.ToCharArray(),
                numericChars.ToCharArray(),
                specialChars.ToCharArray()
            };
            var charsLeftInGroup = new int[charGroups.Length];
            for (var i = 0; i < charsLeftInGroup.Length; i++)
            {
                charsLeftInGroup[i] = charGroups[i].Length;
            }
            var leftGroupsOrder = new int[charGroups.Length];
            for (var i = 0; i < leftGroupsOrder.Length; i++)
            {
                leftGroupsOrder[i] = i;
            }
            var randomBytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var seed = BitConverter.ToInt32(randomBytes, 0);
            var random = new Random(seed);
            var password = new char[length];
            var lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
            for (var i = 0; i < password.Length; i++)
            {
                var nextLeftGroupsOrderIdx = lastLeftGroupsOrderIdx == 0 ? 0 : random.Next(0, lastLeftGroupsOrderIdx);
                var nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];
                var lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;
                var nextCharIdx = lastCharIdx == 0 ? 0 : random.Next(0, lastCharIdx + 1);
                password[i] = charGroups[nextGroupIdx][nextCharIdx];
                if (lastCharIdx == 0)
                {
                    charsLeftInGroup[nextGroupIdx] = charGroups[nextGroupIdx].Length;
                }
                else
                {
                    if (lastCharIdx != nextCharIdx)
                    {
                        var temp = charGroups[nextGroupIdx][lastCharIdx];
                        charGroups[nextGroupIdx][lastCharIdx] = charGroups[nextGroupIdx][nextCharIdx];
                        charGroups[nextGroupIdx][nextCharIdx] = temp;
                    }
                    charsLeftInGroup[nextGroupIdx]--;
                }
                if (lastLeftGroupsOrderIdx == 0)
                {
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                }
                else
                {
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        var temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] = leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    lastLeftGroupsOrderIdx--;
                }
            }
            return new string(password);
        }

    }
}
