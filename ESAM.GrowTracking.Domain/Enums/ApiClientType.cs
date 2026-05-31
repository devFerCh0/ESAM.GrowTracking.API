using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Domain.Enums
{
    public enum ApiClientType : byte
    {
        [Display(Name = "Web")]
        [EnumMember(Value = "Web")]
        Web = 1,

        [Display(Name = "Mobile")]
        [EnumMember(Value = "Mobile")]
        Mobile = 2,

        [Display(Name = "Desktop")]
        [EnumMember(Value = "Desktop")]
        Desktop = 3
    }
}