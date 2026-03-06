namespace XeniaQLaunchBackend.Dto
{
    public class MswipePaymentResponse
    {
        public string txn_id { get; set; }
        public string status { get; set; }
        public string smslink { get; set; }
        public string MessageContent { get; set; }
        public string ExtraNote1 { get; set; }
        public string ExtraNote2 { get; set; }
        public string ExtraNote3 { get; set; }
        public string ExtraNote4 { get; set; }
        public string ExtraNote5 { get; set; }
        public string responsecode { get; set; }
        public string responsemessage { get; set; }
    }
}
