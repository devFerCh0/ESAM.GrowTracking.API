namespace ESAM.GrowTracking.Infrastructure.Abstractions.Security
{
    public interface ITokenProtector
    {
        string Protect(string plaintext);

        bool TryUnprotect(string ciphertext, out string? plaintext);
    }
}