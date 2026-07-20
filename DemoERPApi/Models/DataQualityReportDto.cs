namespace DemoERPApi.Models;


/// DTO for data quality report statistics.

public class DataQualityReportDto
{
    public int MissingEmailCount { get; set; }
    public int MissingPhoneCount { get; set; }
    public int MissingNameCount { get; set; }
}