using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace YAppiWinService
{
    public static class SimpleDataProtectorManager
    {
        public static string Encrypt(string text)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = ProtectedData.Protect(clearBytes, null, DataProtectionScope.LocalMachine);
            return Convert.ToBase64String(encryptedBytes);
        }

        public static string Decrypt(string text)
        {
            byte[] encryptedBytes = Convert.FromBase64String(text);
            byte[] clearBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(clearBytes);
        }
    }
}