using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace InfoSecurity
{
    public static class SecurityHelper
    {
        public static string GetHash(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            using (var algorithm = SHA256.Create())
            {
                byte[] bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                return string.Concat(bytes.Select(x => x.ToString("X2")));
            }
        }

        public static bool IsPasswordValid(string password, out string error)
        {
            error = null;
            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                error = "Пароль должен содержать не менее 6 символов.";
                return false;
            }

            bool hasDigit = false;
            foreach (char c in password)
            {
                bool latin = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
                bool digit = c >= '0' && c <= '9';
                hasDigit |= digit;
                if (!latin && !digit)
                {
                    error = "Используйте только английские буквы и цифры без пробелов.";
                    return false;
                }
            }

            if (!hasDigit)
            {
                error = "Добавьте в пароль хотя бы одну цифру.";
                return false;
            }

            return true;
        }
    }
}
