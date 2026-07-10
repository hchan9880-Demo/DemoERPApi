namespace DemoERPApi.Tests.Helpers;

public static class TestData
{
    // Existing base system records mapped from your database seed script
    public static string ExistingCustomerID = "CRM100";  // Matches Admin/QA default baseline
    public static string ExistingCustomerID2 = "CRM101"; // Sarah's customer
    public static string OwnerCustomerID = "CRM103";     // Owner1's assigned customer
    public static string OtherCustomerID = "CRM300";     // Admin fallback customer

    // Explicit negative assertion boundaries
    public static string NonExistingCustomerID = "CRM999";

    // Dynamic Context-Driven String Identifiers (Clean Organization Targets)
    public static class Admin
    {
        public const string GetId = "CRM_ADMIN_GET_011A";
        public const string PutId = "CRM_ADMIN_PUT_011B";
        public const string DeleteId = "CRM_ADMIN_DEL_011C";
        public const string SyncId = "CRM_ADMIN_SYNC_011D";
        public const string OwnGetId = "CRM_ADMIN_GET_011E";
        public const string OwnPutId = "CRM_ADMIN_PUT_011F";
        public const string OwnDeleteId = "CRM_ADMIN_DEL_011G";
    }

    public static class Customer
    {
        public const string OtherCustomerId = "CRM_OTHER_999";
        public const string NewCustomerId = "CRM_NEW_888";
    }

    public static class QA
    {
        public const string OtherQaCustomerId = "CRM_QA_OTHER_101";
        public const string OutOfScopeCustomerId = "CRM_OUT_OF_SCOPE_202";
    }
}