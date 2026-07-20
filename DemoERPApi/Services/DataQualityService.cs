using System.Collections.Generic;

namespace DemoERPApi.Services;


/// Data quality validation result.

public class DataQualityResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}


/// Service for validating customer data quality.

public class DataQualityService
{
    
    /// Validates customer email and phone.
    
    public DataQualityResult ValidateCustomer(string email, string phone)
    {
        var result = new DataQualityResult();

        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            result.Errors.Add("Invalid email format");

        if (string.IsNullOrWhiteSpace(phone))
            result.Errors.Add("Phone number missing");

        result.IsValid = result.Errors.Count == 0;
        return result;
    }
}