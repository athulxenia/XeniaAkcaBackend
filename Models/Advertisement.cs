using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XeniaAkcaBackend.Models
{
    [Table("AKCA_Advertisement")]
    public class Advertisement
    {
        [Key]
        [Column("advertisementId")]
        public int AdvertisementId { get; set; }

        [Column("advertisementName")]
        public string AdvertisementName { get; set; } = string.Empty;

        [Column("districtId")]
        public int DistrictId { get; set; }

        [Column("fileUrl")]
        public string FileUrl { get; set; } = string.Empty;

        [Column("advertisementCreatedUser")]
        public string AdvertisementCreatedUser { get; set; } = string.Empty;

        [Column("advertisementCreatedDate")]
        public DateTime AdvertisementCreatedDate { get; set; }

        [Column("advertisementModifiedUser")]
        public string? AdvertisementModifiedUser { get; set; }

        [Column("advertisementModifiedDate")]
        public DateTime? AdvertisementModifiedDate { get; set; }

        [Column("advertisementApprovedUser")]
        public string? AdvertisementApprovedUser { get; set; }

        [Column("advertisementApprovedDate")]
        public DateTime? AdvertisementApprovedDate { get; set; }

        [Column("advertisementContent")]
        public string AdvertisementContent { get; set; } = string.Empty;

        [Column("advertisementStartDate")]
        public DateTime AdvertisementStartDate { get; set; }

        [Column("advertisementEndDate")]
        public DateTime AdvertisementEndDate { get; set; }

        [Column("advertisementStatus")]
        public string AdvertisementStatus { get; set; } = string.Empty;

        [Column("activeStatus")]
        public bool ActiveStatus { get; set; }

        [Column("advertisementApproveStatus")]
        public int? AdvertisementApproveStatus { get; set; }
    }
}