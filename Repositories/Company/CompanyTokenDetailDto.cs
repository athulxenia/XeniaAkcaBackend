using XeniaTokenBackend.Dto;

namespace XeniaTokenBackend.Repositories.Company
{
    public class CompanyTokenDetailDto
    {
        public CompanyTokenListDto Company { get; set; } = null!;
        public CompanyTokenSettingsDto Settings { get; set; } = null!;
    }
}