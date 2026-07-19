namespace DemoERPApi.Models
{
    public class SystemHealthReportDto
    {
        public string ApiStatus { get; set; }

        public string DatabaseStatus { get; set; }

        public DateTime ServerTime { get; set; }
    }
}