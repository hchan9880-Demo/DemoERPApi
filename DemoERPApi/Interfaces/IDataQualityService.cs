using DemoERPApi.Models;

namespace DemoERPApi.Interfaces
{
    public interface IDataQualityService
    {
        DataQualityResult ValidateCustomer(CustomersDto customer);
    }

}
