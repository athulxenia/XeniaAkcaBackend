namespace XeniaTokenBackend.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("xtm_Customer", Schema = "dbo")]
    public class xtm_Customer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerID { get; set; }

        [Required]
        [MaxLength(255)]
        public string CustomerName { get; set; }

        [MaxLength(20)]
        public string CustomerMobileNumber { get; set; }

        public bool Status { get; set; } = true;
    }

}
