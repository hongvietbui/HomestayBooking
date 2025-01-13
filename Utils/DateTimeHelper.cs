namespace EXE202.Utils
{
    public static class DateTimeHelper
    {
        public static string GetFormatedDateTime(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm:ss");
        }

        public static string GetFormatedDate(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy");
        }
    }
}
