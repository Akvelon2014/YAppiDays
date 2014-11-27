using System;
using System.Security.Cryptography;
using System.Text;
using MobileConference.Interface;

namespace MobileConference.Managers
{
    public class Sha256CryptManager : ICryptManager
    {
        private static readonly SHA256 Md256Hash = SHA256.Create();

        public string GetHash(string inputData)
        {
            var stringBuilder = new StringBuilder();
            byte[] data = Md256Hash.ComputeHash(Encoding.UTF8.GetBytes(inputData));
            foreach (byte byteOfData in data)
            {
                stringBuilder.Append(byteOfData.ToString("x2"));
            }
            return stringBuilder.ToString();
        }

        public bool VerifyHash(string inputData, string storingHash)
        {
            string hashOfInput = GetHash(inputData);
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            return comparer.Compare(hashOfInput, storingHash) == 0;
        }
    }
}