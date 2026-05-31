using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Domain.Enums
{
    public enum IdentityDocumentType : byte
    {
        [Display(Name = "IdentityCard")]
        [EnumMember(Value = "IdentityCard")]
        IdentityCard = 1
    }
}