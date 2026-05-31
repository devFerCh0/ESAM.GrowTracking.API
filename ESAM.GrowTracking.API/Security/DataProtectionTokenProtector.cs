using ESAM.GrowTracking.Infrastructure.Abstractions.Security;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;

namespace ESAM.GrowTracking.API.Security
{
    public sealed class DataProtectionTokenProtector : ITokenProtector
    {
        private const string ProtectorPurpose = "ESAM.GrowTracking.Security.RefreshTokenProtector.v1";
        private readonly IDataProtector _protector;

        public DataProtectionTokenProtector(IDataProtectionProvider dataProtectionProvider)
        {
            ArgumentNullException.ThrowIfNull(dataProtectionProvider);
            _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        }

        public string Protect(string plaintext)
        {
            if (string.IsNullOrWhiteSpace(plaintext))
                throw new ArgumentException("El texto plano no puede ser nulo o vacío.", nameof(plaintext));
            return _protector.Protect(plaintext);
        }

        public bool TryUnprotect(string ciphertext, out string? plaintext)
        {
            plaintext = null;
            if (string.IsNullOrWhiteSpace(ciphertext))
                return false;
            try
            {
                var result = _protector.Unprotect(ciphertext);
                if (string.IsNullOrWhiteSpace(result))
                    return false;
                plaintext = result;
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
        }
    }
}