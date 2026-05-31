using ESAM.GrowTracking.Domain.Abstractions;
using ESAM.GrowTracking.Domain.Enums;
using ESAM.GrowTracking.Domain.Primitives;

namespace ESAM.GrowTracking.Domain.Entities
{
    public sealed class Person : AuditableEntity, IEntity<int>
    {
        private Person() { }

        public int Id { get; private set; }

        public string FirstName { get; private set; } = string.Empty;

        public string LastName { get; private set; } = string.Empty;

        public string? SecondLastName { get; private set; }

        public string IdentityDocument { get; private set; } = string.Empty;

        public IdentityDocumentType IdentityDocumentType { get; private set; }

        public Gender Gender { get; private set; }

        public MaritalStatus MaritalStatus { get; private set; }

        public bool IsDeleted { get; private set; }

        public byte[] RecordVersion { get; private set; } = null!;

        public User User { get; private set; } = null!;

        public Person(int id, string firstName, string lastName, string? secondLastName, string identityDocument, IdentityDocumentType identityDocumentType, Gender gender, 
            MaritalStatus maritalStatus, int createdBy, DateTime? createdAt = null)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            SecondLastName = secondLastName;
            IdentityDocument = identityDocument;
            IdentityDocumentType = identityDocumentType;
            Gender = gender;
            MaritalStatus = maritalStatus;
            SetCreatedAudit(createdBy, createdAt);
        }

        //public void Update(string firstName, string lastName, string? secondLastName, string identityDocument, IdentityDocumentType identityDocumentType, Gender gender,
        //    MaritalStatus maritalStatus, int updatedBy)
        //{
        //    FirstName = firstName;
        //    LastName = lastName;
        //    SecondLastName = secondLastName;
        //    IdentityDocument = identityDocument;
        //    IdentityDocumentType = identityDocumentType;
        //    Gender = gender;
        //    MaritalStatus = maritalStatus;
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