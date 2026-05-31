using ESAM.GrowTracking.API.Settings;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ESAM.GrowTracking.API.ConfigureOptions
{
    internal sealed class KeyManagementOptionsSetup : IConfigureOptions<KeyManagementOptions>
    {
        private readonly IHostEnvironment _environment;
        private readonly DataProtectionSettings _dataProtectionSettings;

        public KeyManagementOptionsSetup(IHostEnvironment environment, IOptions<DataProtectionSettings> dataProtectionSettingsOptions)
        {
            ArgumentNullException.ThrowIfNull(environment);
            ArgumentNullException.ThrowIfNull(dataProtectionSettingsOptions);
            _environment = environment;
            _dataProtectionSettings = dataProtectionSettingsOptions.Value ?? throw new ArgumentNullException(nameof(dataProtectionSettingsOptions));
        }

        public void Configure(KeyManagementOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            var keysPath = _dataProtectionSettings.KeysPath;
            if (string.IsNullOrWhiteSpace(keysPath))
                throw new InvalidOperationException("DataProtection:KeysPath no está configurado.");
            var fullPath = Path.IsPathRooted(keysPath) ? Path.GetFullPath(keysPath) : Path.GetFullPath(Path.Combine(_environment.ContentRootPath, keysPath));
            try
            {
                Directory.CreateDirectory(fullPath);
                RestrictKeysDirectoryPermissions(fullPath);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"DataProtection: No se pudo preparar el almacenamiento de claves en '{fullPath}'.", ex);
            }
            options.XmlRepository = new FileSystemXmlRepository( new DirectoryInfo(fullPath), NullLoggerFactory.Instance);
        }

        private static void RestrictKeysDirectoryPermissions(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var dirInfo = new DirectoryInfo(path);
                var security = dirInfo.GetAccessControl();
                security.SetAccessRuleProtection(true, false);
                var currentUser = WindowsIdentity.GetCurrent().Name;
                security.AddAccessRule(new FileSystemAccessRule(currentUser, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                    PropagationFlags.None,  AccessControlType.Allow));
                dirInfo.SetAccessControl(security);
            }
        }
    }
}