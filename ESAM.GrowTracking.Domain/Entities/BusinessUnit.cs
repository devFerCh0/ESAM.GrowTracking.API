using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class BusinessUnit : AuditableEntity, IEntity<int>
    {
        private readonly List<Campus> _campuses = [];

        private BusinessUnit() { }

        public int Id { get; private set; }

        public string Name { get; private set; } = string.Empty;

        public string Abbreviation { get; private set; } = string.Empty;

        public string WebSite { get; private set; } = string.Empty;

        public DateTime FoundingDate { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public IReadOnlyCollection<Campus> Campuses => _campuses.AsReadOnly();

        public BusinessUnit(int id, string name, string abbreviation, string webSite, DateTime foundingDate, int createdBy, DateTime? createdAt = null)
        {
            Id = id;
            Name = name;
            Abbreviation = abbreviation;
            WebSite = webSite;
            FoundingDate = foundingDate;
            SetCreatedAudit(createdBy, createdAt);
        }
    }
}