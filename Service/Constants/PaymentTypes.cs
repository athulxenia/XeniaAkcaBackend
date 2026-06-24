namespace XeniaKhraBackend.Service.Constants
{
    public static class PaymentTypes
    {
        public const int Regular = 2;
        public const int Contribution = 3;
        public const int Other = 4;

        public static bool IsContributionPayment(int paymentTypeId)
        {
            return paymentTypeId == Contribution || paymentTypeId == Other;
        }

        public static bool IsRegularPayment(int paymentTypeId)
        {
            return paymentTypeId == Regular;
        }
    }
}
