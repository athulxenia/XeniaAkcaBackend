namespace XeniaQLaunchBackend.Dto
{
    public class MswipeTransactionStatusResponse
    {
        public string Status { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public List<MswipeTransactionData> Data { get; set; }
    }
}
