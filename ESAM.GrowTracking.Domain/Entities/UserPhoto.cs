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

        //public UserPhoto(int userId, string photo, bool isDefault, int createdBy)
        //{
        //    UserId = userId;
        //    Photo = photo;
        //    IsDefault = isDefault;
        //    SetCreatedAudit(createdBy);
        //}

        //public void SetAsDefault(int updatedBy)
        //{
        //    IsDefault = true;
        //    SetUpdatedAudit(updatedBy);
        //}

        //public void UnsetDefault(int updatedBy)
        //{
        //    IsDefault = false;
        //    SetUpdatedAudit(updatedBy);
        //}

        //public void SoftDelete(int updatedBy)
        //{
        //    IsDeleted = true;
        //    SetUpdatedAudit(updatedBy);
        //}
    }
}