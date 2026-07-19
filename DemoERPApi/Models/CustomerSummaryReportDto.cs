namespace DemoERPApi.Models
{
    public class CustomerSummaryReportDto
    {
        public int TotalCustomers { get; set; }

        public int ActiveCustomers { get; set; }

        public int DeletedCustomers { get; set; }
    }
}