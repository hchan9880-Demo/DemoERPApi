using DemoERPApi.Data;
using DemoERPApi.Services;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DemoERPApi.Tests.Services
{
    public class ReportingServiceTests
    {
        private readonly AppDbContext _context;


        public ReportingServiceTests()
        {
            var options =
                new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(
                    databaseName: Guid.NewGuid().ToString()
                )
                .Options;


            _context =
                new AppDbContext(options);


            SeedData();
        }



        private void SeedData()
        {
            _context.Customers.AddRange(

                new Customers
                {
                   // CustomerID = "1",
                    CRMCustomerID = "CRM100",
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john@test.com",
                    Phone = "1234567890",
                    IsDeleted = false
                },


                new Customers
                {
               //     CustomerID = "2",
                    CRMCustomerID = "CRM101",
                    FirstName = "Mary",
                    LastName = "Lee",
                    Email = "mary@test.com",
                    Phone = "5555555555",
                    IsDeleted = true
                }

            );


            _context.SaveChanges();
        }



        private ReportingService CreateReportingService()
        {
            return new ReportingService(_context);
        }




        [Fact]
        public async Task GetCustomerSummary_ShouldReturnCorrectCounts()
        {
            // Arrange
            var service =
                CreateReportingService();


            // Act
            var result =
                await service.GetCustomerSummaryAsync();



            // Assert
            Assert.NotNull(result);


            Assert.Equal(
                2,
                result.TotalCustomers
            );


            Assert.Equal(
                1,
                result.ActiveCustomers
            );


            Assert.Equal(
                1,
                result.DeletedCustomers
            );
        }
    }
}