using System;
using System.Security.Cryptography;
using System.Text;

namespace ReServer.Core
{
    public static class PasswordCrypt
    {
        public static string Encrypt(string password, string username)
        {
            var salt = Encoding.UTF8.GetBytes("rfc2898-syrup-" + username);

            var crypt = new Rfc2898DeriveBytes(password, salt, 1000);
            var key = crypt.GetBytes(16);

            return Convert.ToBase64String(key);
        }
    }
}
