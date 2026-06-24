using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Users")]
    public class User
    {
        [Key]
        [Column("userId")]
        public int UserId { get; set; }

        [Column("companyId")]
        public int CompanyId { get; set; }

        [Column("userGroupId")]
        public int? UserGroupId { get; set; }

        [Column("userDistrictId")]
        public int? UserDistrictId { get; set; }

        [Column("userUnitId")]
        public int? UserUnitId { get; set; }

        [Column("userName")]
        public string? UserName { get; set; }

        [Column("password")]
        public string? Password { get; set; }

        [Column("userImageUrl")]
        public string? UserImageUrl { get; set; }

        [Column("userStatus")]
        public bool UserStatus { get; set; }                

        [Column("firebaseToken")]
        public string? FirebaseToken { get; set; }

        [Column("UserCreatedOn")]
        public DateTime? UserCreatedOn { get; set; }
    }
}