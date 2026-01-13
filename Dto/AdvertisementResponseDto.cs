namespace XeniaTokenBackend.Dto
{

    public class AdvertisementResponseDto
    {
        public string Status { get; set; }
        public List<AdvertisementDto> Advertisement { get; set; }
    }
    public class AdvertisementDto
    {
        public int AdvID { get; set; }
        public int CompanyID { get; set; }
        public int DepID { get; set; }
        public string AdvName { get; set; }
        public int AdvOrder { get; set; }
        public string AdvFileUrl { get; set; }
        public DateTime AdvModifiedDate { get; set; }
        public int AdvModifiedUserID { get; set; }
        public bool Status { get; set; }
        public string DepName { get; set; } 
    }

}
