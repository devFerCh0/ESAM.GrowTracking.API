namespace ESAM.GrowTracking.Application.Abstractions.Services
{
    public interface IHashService
    {
        string ComputeHash(string input, string salt, int? iterations = null, int? hashSize = null);

        string GenerateSalt(int size = 0);

        bool VerifyHash(string input, string salt, string expectedHash, int? iterations = null, int? hashSize = null);
    }
}