using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserDevice : AuditableEntity, IEntity<int>
    {
        private readonly List<UserSession> _userSessions = [];

        private UserDevice() { }

        public int Id { get; private set; }

        public int UserId { get; private set; }

        public string DeviceIdentifier { get; private set; } = string.Empty;

        public string DeviceName { get; private set; } = string.Empty;

        public ApiClientType ApiClientType { get; private set; }

        public bool IsTrusted { get; private set; }

        public DateTime? LastSeenAt { get; private set; }

        public string? LastIp { get; private set; }

        public string? LastUserAgent { get; private set; }

        public bool IsDeleted { get; private set; }

        public int FailedLoginCount { get; private set; }

        public DateTime? LastFailedLoginAt { get; private set; }

        public DateTime? LockoutEndAt { get; private set; }

        public DateTime? LastLoginAt { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public IReadOnlyCollection<UserSession> UserSessions => _userSessions.AsReadOnly();

        public UserDevice(int userId, string deviceIdentifier, string deviceName, ApiClientType apiClientType, string? lastIp, string? lastUserAgent, int createdBy,
            DateTime? createdAt = null)
        {
            UserId = userId;
            DeviceIdentifier = deviceIdentifier.Trim();
            DeviceName = deviceName.Trim();
            ApiClientType = apiClientType;
            LastIp = lastIp;
            LastUserAgent = lastUserAgent;
            SetCreatedAudit(createdBy, createdAt);
        }

        public void Activate(int updatedBy, DateTime? updatedAt = null)
        {
            IsDeleted = false;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void Update(string deviceName, ApiClientType apiClientType, string? lastIp, string? lastUserAgent, int updatedBy, DateTime? updatedAt = null)
        {
            DeviceName = deviceName.Trim();
            ApiClientType = apiClientType;
            LastIp = lastIp;
            LastUserAgent = lastUserAgent;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void UpdateLastSeenAt(DateTime lastSeenAt, int updatedBy, DateTime? updatedAt = null)
        {
            LastSeenAt = lastSeenAt;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public bool ShouldResetFailedAttempts(TimeSpan failedAttemptsResetDuration, DateTime utcNow) =>
            LastFailedLoginAt.HasValue && (utcNow - LastFailedLoginAt.Value) > failedAttemptsResetDuration;

        public void ResetFailedLogin(int updatedBy, DateTime? updatedAt = null)
        {
            FailedLoginCount = 0;
            LastFailedLoginAt = null;
            LockoutEndAt = null;
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public bool IsLocked(DateTime utcNow) => LockoutEndAt.HasValue && LockoutEndAt.Value > utcNow;


        public void RegisterFailedLogin(int maxFailedAttempts, TimeSpan lockoutDuration, DateTime lastFailedLoginAt, int updatedBy, DateTime? updatedAt = null)
        {
            FailedLoginCount++;
            LastFailedLoginAt = lastFailedLoginAt;
            if (FailedLoginCount >= maxFailedAttempts)
                LockoutEndAt = lastFailedLoginAt.Add(lockoutDuration);
            SetUpdatedAudit(updatedBy, updatedAt);
        }

        public void UpdateLastLogin(DateTime lastLoginAt, int updatedBy, DateTime? updatedAt = null)
        {
            LastLoginAt = lastLoginAt;
            SetUpdatedAudit(updatedBy, updatedAt);
        }
    }
}