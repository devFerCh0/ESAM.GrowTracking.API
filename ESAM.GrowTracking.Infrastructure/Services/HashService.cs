using ESAM.GrowTracking.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ESAM.GrowTracking.Infrastructure.Services
{
    public sealed class HashService : IHashService, IDisposable
    {
        private readonly ILogger<HashService> _logger;
        private readonly RandomNumberGenerator _rng;
        private readonly int _defaultSaltSize;
        private readonly int _defaultIterations;
        private readonly int _defaultHashSize;
        private bool _disposed;
        private const int RecommendedMinIterations = 600_000;

        public HashService(ILogger<HashService> logger, RandomNumberGenerator? rng = null, int defaultSaltSize = 16, int defaultIterations = RecommendedMinIterations, 
            int defaultHashSize = 32)
        {
            ArgumentNullException.ThrowIfNull(logger);
            if (defaultSaltSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultSaltSize), "defaultSaltSize debe ser mayor a cero.");
            if (defaultIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultIterations), "defaultIterations debe ser mayor a cero.");
            if (defaultHashSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(defaultHashSize), "defaultHashSize debe ser mayor a cero.");
            _logger = logger;
            _defaultSaltSize = defaultSaltSize;
            _defaultIterations = defaultIterations;
            _defaultHashSize = defaultHashSize;
            _rng = rng ?? RandomNumberGenerator.Create();
        }

        public string GenerateSalt(int size = 0)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (size < 0)
                throw new ArgumentOutOfRangeException(nameof(size), "El tamaño debe ser cero (usar el predeterminado) o un entero positivo.");
            var actualSize = size > 0 ? size : _defaultSaltSize;
            var saltBytes = new byte[actualSize];
            _rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public string ComputeHash(string input, string salt, int? iterations = null, int? hashSize = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(salt);
            var iterCount = iterations ?? _defaultIterations;
            var derivedLen = hashSize ?? _defaultHashSize;
            if (iterCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterations), "El número de iteraciones debe ser mayor a cero.");
            if (derivedLen <= 0)
                throw new ArgumentOutOfRangeException(nameof(hashSize), "El tamaño del hash debe ser mayor a cero.");
            byte[] saltBytes;
            try
            {
                saltBytes = Convert.FromBase64String(salt);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "ComputeHash: la sal proporcionada no es Base64 válido.");
                throw new FormatException("La sal proporcionada no es una cadena Base64 válida.", ex);
            }
            using var pbkdf2 = new Rfc2898DeriveBytes(input, saltBytes, iterCount, HashAlgorithmName.SHA256);
            var hashBytes = pbkdf2.GetBytes(derivedLen);
            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyHash(string input, string salt, string expectedHash, int? iterations = null, int? hashSize = null)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            ArgumentNullException.ThrowIfNull(expectedHash);
            byte[] expectedBytes;
            try
            {
                expectedBytes = Convert.FromBase64String(expectedHash);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "VerifyHash: el hash esperado proporcionado no es Base64 válido.");
                throw new FormatException("El hash esperado proporcionado no es una cadena Base64 válida.", ex);
            }
            var computedHash = ComputeHash(input, salt, iterations, hashSize);
            var computedBytes = Convert.FromBase64String(computedHash);
            return CryptographicOperations.FixedTimeEquals(computedBytes, expectedBytes);
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            _rng.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}