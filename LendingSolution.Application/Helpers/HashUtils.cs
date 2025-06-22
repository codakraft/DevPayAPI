using System.Security.Cryptography;
using System.Text;

namespace LendingSolution.Application.Helpers;

public static class HashUtils
{
    public static string ComputeSha512Hash(string rawData)
    {
        using var sha512 = SHA512.Create();
        var bytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();

        foreach (var b in bytes)
        {
            builder.Append(b.ToString("x2")); // convert to lowercase hex
        }

        return builder.ToString();
    }
}
