namespace DemoERPApi.Models;


/// Result model for data quality validation.

public class DataQualityResult
{
    public bool IsValid { get; set; }
    public int TotalIssues { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}