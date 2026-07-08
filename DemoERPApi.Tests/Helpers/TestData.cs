namespace DemoERPApi.Tests.Helpers;

public static class TestData
{
    // Existing customer owned by another user
    public static string ExistingCustomerID = "CRM001";


    // Another existing customer
    public static string ExistingCustomerID2 = "CRM101";


    // Owner1's assigned customer
    public static string OwnerCustomerID = "CRM103";


    // Existing customer NOT owned by owner1
    public static string OtherCustomerID = "CRM100";


    // Invalid customer
    public static string NonExistingCustomerID = "CRM999";
}