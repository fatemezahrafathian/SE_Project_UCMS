using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Konscious.Security.Cryptography;
using UCMS.Services.AuthService.Abstraction;

namespace UCMS.Services.AuthService;

public class PasswordService: IPasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 4;
    private const int MemorySize = 65536;
    private const int Parallelism = 2;

    public bool IsPasswordValid(string password)
    {
        // at least 8 characters length, containing at least a capital letter, an small letter, a number and a special characters
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
        Regex regex = new Regex(pattern);
        return regex.IsMatch(password);
    }

    public byte[] CreateSalt()
    {
        byte[] saltBytes = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(saltBytes);
        }
        return saltBytes;
    }

    public async Task<byte[]> HashPasswordAsync(string password, byte[] salt)
    {
        return await Task.Run(() =>
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = Parallelism;
                argon2.MemorySize = MemorySize;
                argon2.Iterations = Iterations;
                return argon2.GetBytes(HashSize);
            }
        });
    }

    public async Task<bool> VerifyPasswordAsync(string password, byte[] salt, byte[] hashedPassword)
    {
        byte[] newHash = await HashPasswordAsync(password, salt);
        return newHash.SequenceEqual(hashedPassword);
    }
}