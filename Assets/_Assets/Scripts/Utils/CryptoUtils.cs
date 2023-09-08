using System.Security.Cryptography;
using System.Text;

public static class CryptoUtils
{
    public static string GenerateSHA256Hash(string input)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2")); // Convert each byte to a two-digit hexadecimal representation
            }

            return sb.ToString();
        }
    }
}