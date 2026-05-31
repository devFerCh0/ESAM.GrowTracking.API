namespace ESAM.GrowTracking.API.Controllers.Auth.Login
{
    public record LoginRequest
    {
        public string? Credential { get; init; }

        public string? Password { get; init; }

        public bool? IsPersistent { get; init; }

        public string? DeviceIdentifier { get; init; }

        public string? DeviceName { get; init; }

        public string? ApiClientType { get; init; }

        public LoginRequest(string? credential, string? password, bool? isPersistent, string? deviceIdentifier, string? deviceName, string? apiClientType)
        {
            Credential = credential;
            Password = password;
            IsPersistent = isPersistent;
            DeviceIdentifier = deviceIdentifier;
            DeviceName = deviceName;
            ApiClientType = apiClientType;
        }
    }
}