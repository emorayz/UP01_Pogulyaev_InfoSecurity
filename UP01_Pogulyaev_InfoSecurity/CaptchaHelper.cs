using System;
using System.Text;

namespace InfoSecurity
{
    public static class CaptchaHelper
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private static readonly Random Random = new Random();

        public static string Generate(int length = 6)
        {
            var result = new StringBuilder(length);
            for (int i = 0; i < length; i++)
                result.Append(Alphabet[Random.Next(Alphabet.Length)]);
            return result.ToString();
        }

        public static bool Verify(string expected, string actual)
        {
            return !string.IsNullOrWhiteSpace(actual) &&
                   string.Equals(expected, actual.Trim(), StringComparison.Ordinal);
        }
    }
}
