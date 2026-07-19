namespace DemoERPApi.Models
{
    public class SyncReportDto
    {
        public int CustomerId { get; set; }

        public string CRMCustomerID { get; set; }

        public string Status { get; set; }

        public DateTime SyncDate { get; set; }
    }
}