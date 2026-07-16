using DemoERPApi.Data;
using DemoERPApi.Exceptions;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoERPApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ILoggingService _loggingService;


        public CustomerService(
            AppDbContext context,
            IAuditService auditService,
            ILoggingService loggingService)
        {
            _context = context;
            _auditService = auditService;
            _loggingService = loggingService;
        }



        public async Task<Customer> SyncCustomerAsync(
            CustomerDto dto,
            string userName)
        {

            // ===============================
            // Validation
            // ===============================

            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new ValidationException(
                    "Email is required");
            }


            // ===============================
            // Duplicate Check
            // ===============================

            var existing =
                await _context.Customers
                .FirstOrDefaultAsync(
                    x => x.CRMCustomerID
                    == dto.CRMCustomerID);


            if (existing != null)
            {
                throw new BusinessException(
    "CUSTOMER_DUPLICATE",
    "Customer already exists");
            }



            // ===============================
            // Create Customer
            // ===============================

            var customer = new Customer
            {
                CRMCustomerID =
                    dto.CRMCustomerID,

                FirstName =
                    dto.FirstName,

                LastName =
                    dto.LastName,

                Email =
                    dto.Email,

                Phone =
                    dto.Phone,

                LastUpdated =
                    DateTime.UtcNow
            };


            _context.Customers.Add(customer);



            // ===============================
            // Sync Log
            // ===============================

            var syncLog = new SyncLogs
            {
                CRMCustomerID =
                    dto.CRMCustomerID,

               // Action = "CREATE",

                Status = "SUCCESS",

                Message =
                    "Customer created successfully",

                Username =
                    userName,

                CreatedDate =
                    DateTime.UtcNow
            };


            _context.SyncLogs.Add(syncLog);



            // ===============================
            // Audit Log
            // ===============================

            var audit = new AuditLog
            {
           //     UserName =
           //         userName,

                Action =
                    "CREATE",

                EntityName =
                    "Customer",

                EntityId =
                    dto.CRMCustomerID,

                ChangedDate =
                    DateTime.UtcNow
            };


            _context.AuditLogs.Add(audit);



            // ===============================
            // Save Everything
            // ===============================

            await _context.SaveChangesAsync();


            return customer;
        }





        public async Task<Customer?> GetCustomerAsync(
            string crmCustomerId)
        {

            var customer =
                await _context.Customers
                .FirstOrDefaultAsync(
                    x =>
                    x.CRMCustomerID ==
                    crmCustomerId
                    &&
                    !x.IsDeleted);


            if (customer == null)
            {
                throw new NotFoundException(
                    "Customer not found");
            }


            return customer;
        }






        public async Task<bool> DeleteCustomerAsync(
            string crmCustomerId)
        {

            var customer =
                await _context.Customers
                .FirstOrDefaultAsync(
                    x =>
                    x.CRMCustomerID ==
                    crmCustomerId);


            if (customer == null)
            {
                throw new NotFoundException(
                    "Customer not found");
            }


            customer.IsDeleted = true;

            customer.LastUpdated =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();


            return true;
        }
    }
}