namespace XeniaTokenBackend.Service.Common
{
    public class CommonService
    {
  
        public static DateTime FormatDateToLocalTime(DateTime? date = null)
        {
            return (date ?? DateTime.UtcNow).Date;
        }


        public static string ConvertTo12HourFormat(string dateTimeString)
        {
            var utcTime = DateTime.Parse(dateTimeString).ToUniversalTime();
            return utcTime.ToString("h:mm tt");
        }


    }
}
