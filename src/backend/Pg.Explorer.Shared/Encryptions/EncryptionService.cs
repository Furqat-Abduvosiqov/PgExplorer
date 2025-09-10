using System.Security.Cryptography;
using System.Text;

namespace Pg.Explorer.Shared.Encryptions;

public class EncryptionService : IEncryptionService
{
    private const string AesKeyEnvVar = "PGEXPLORER_AES_KEY"; // Base64-encoded 32-byte key

    public string EncryptPassword(string password)
    {
        if (password == null) throw new ArgumentNullException(nameof(password));

        if (OperatingSystem.IsWindows())
        {
            var plainBytes = Encoding.UTF8.GetBytes(password);
            var entropy = Encoding.UTF8.GetBytes("PgExplorer-Entropy-v1");
            var protectedBytes = ProtectedData.Protect(plainBytes, entropy, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(protectedBytes);
        }

        // Non-Windows fallback: AES-256-CBC with random IV, key from env
        var key = GetAesKeyFromEnvironment();
        using var aes = Aes.Create();
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        var plaintextBytes = Encoding.UTF8.GetBytes(password);
        var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);

        // Prepend IV (16 bytes) + ciphertext, then Base64
        var output = new byte[aes.IV.Length + ciphertextBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, output, 0, aes.IV.Length);
        Buffer.BlockCopy(ciphertextBytes, 0, output, aes.IV.Length, ciphertextBytes.Length);
        return Convert.ToBase64String(output);
    }

    private static byte[] GetAesKeyFromEnvironment()
    {
        var base64 = Environment.GetEnvironmentVariable(AesKeyEnvVar);
        if (string.IsNullOrWhiteSpace(base64))
        {
            throw new InvalidOperationException(
                $"Environment variable '{AesKeyEnvVar}' must be set to a Base64-encoded 32-byte key on non-Windows platforms.");
        }

        byte[] key;
        try
        {
            key = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException($"Environment variable '{AesKeyEnvVar}' must be valid Base64.");
        }

        if (key.Length != 32)
        {
            throw new InvalidOperationException(
                $"Environment variable '{AesKeyEnvVar}' must decode to exactly 32 bytes (256-bit key). Current length: {key.Length}.");
        }

        return key;
    }
}