using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace ESAM.GrowTracking.Domain.Enums
{
    public enum MaritalStatus : byte
    {
        [Display(Name = "Single")]
        [EnumMember(Value = "Single")]
        Single = 1,

        [Display(Name = "Married")]
        [EnumMember(Value = "Married")]
        Married = 2,

        [Display(Name = "Widower")]
        [EnumMember(Value = "Widower")]
        Widower = 3,

        [Display(Name = "Divorced")]
        [EnumMember(Value = "Divorced")]
        Divorced = 4
    }
}