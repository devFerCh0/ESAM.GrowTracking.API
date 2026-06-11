using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class UserPhoto : AuditableEntity, IEntity<int>
    {
        private UserPhoto() { }

        public int Id { get; private set; }

        public int UserId { get; private set; }

        public string Photo { get; private set; } = string.Empty;

        public bool IsDefault { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;
    }
}