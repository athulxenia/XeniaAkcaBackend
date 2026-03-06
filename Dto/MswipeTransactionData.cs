namespace XeniaQLaunchBackend.Dto
{
    public class MswipeTransactionData
    {
        public string IPG_ID { get; set; }
        public decimal Amount { get; set; }
        public string Cust_Code { get; set; }
        public string MID { get; set; }
        public string TID { get; set; }
        public int Payment_Status { get; set; }
        public string Payment_Desc { get; set; }
        public string Order_Id { get; set; }
        public string Payment_Id { get; set; }
        public string TrxDateTime { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
        public string PaymentType { get; set; }
        public DateTime Created_On { get; set; }
    }
}
