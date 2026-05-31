using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class Campus : AuditableEntity, IEntity<int>
    {
        private readonly List<UserRoleCampus> _userRoleCampuses = [];

        private Campus() { }

        public int Id { get; private set; }

        public int BusinessUnitId { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string WebSite { get; private set; } = string.Empty;

        public DateTime FoundingDate { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public BusinessUnit BusinessUnit { get; private set; } = null!;

        public IReadOnlyCollection<UserRoleCampus> UserRoleCampuses => _userRoleCampuses.AsReadOnly();

        public Campus(int id, int businessUnitId, string name, string webSite, DateTime foundingDate, int createdBy, DateTime? createdAt = null)
        {
            Id = id;
            BusinessUnitId = businessUnitId;
            Name = name;
            WebSite = webSite;
            FoundingDate = foundingDate;
            SetCreatedAudit(createdBy, createdAt);
        }

        //public void Update(string name, string webSite, DateTime foundingDate, int updatedBy)
        //{
        //    Name = name;
        //    WebSite = webSite;
        //    FoundingDate = foundingDate;
        //    SetUpdatedAudit(updatedBy);
        //}

        //public void SoftDelete(int updatedBy)
        //{
        //    IsDeleted = true;
        //    SetUpdatedAudit(updatedBy);
        //}

        //public void Restore(int updatedBy)
        //{
        //    IsDeleted = false;
        //    SetUpdatedAudit(updatedBy);
        //}
    }
}