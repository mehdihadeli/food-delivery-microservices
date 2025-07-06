using System.Security.Cryptography;
using System.Text;

namespace BuildingBlocks.Core.Security;

public static class PkceGenerator
{
    private const int MinLength = 43;
    private const int MaxLength = 128;
    private const int DefaultLength = 64;

    public static PkceCodes Generate(int length = DefaultLength)
    {
        // Validate length requirements
        if (length < MinLength || length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(length),
                $"PKCE code verifier must be between {MinLength}-{MaxLength} characters"
            );
        }

        // Calculate exact byte length needed for desired Base64URL output length
        // Base64 uses 4 chars to represent 3 bytes, so we need 3/4 the characters in bytes
        int byteLength = (int)Math.Ceiling(length * 3 / 4.0);
        byte[] randomBytes = new byte[byteLength];

        // Fill with cryptographically secure random bytes
        RandomNumberGenerator.Fill(randomBytes);

        // Convert to Base64URL and truncate to the exact length
        string codeVerifier = ConvertToBase64Url(randomBytes).AsSpan(0, length).ToString();
        string codeChallenge = ComputeCodeChallenge(codeVerifier);

        return new PkceCodes(codeVerifier, codeChallenge);
    }

    private static string ComputeCodeChallenge(string codeVerifier)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));
        return ConvertToBase64Url(hashBytes);
    }

    private static string ConvertToBase64Url(byte[] input)
    {
        return ConvertToBase64Url(Convert.ToBase64String(input));
    }

    private static string ConvertToBase64Url(string base64)
    {
        return base64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}

public record PkceCodes(string CodeVerifier, string CodeChallenge);
