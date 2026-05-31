using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Domain.Enums
{
    public enum Gender : byte
    {
        [Display(Name = "Man")]
        [EnumMember(Value = "Man")]
        Man = 1,

        [Display(Name = "Woman")]
        [EnumMember(Value = "Woman")]
        Woman = 2
    }
}