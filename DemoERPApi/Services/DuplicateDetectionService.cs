using DemoERPApi.Data;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using DemoERPApi.Interfaces;

namespace DemoERPApi.Services
{
    public class DuplicateDetectionService : IDuplicateDetectionService
    {
        private readonly AppDbContext _context;

        public DuplicateDetectionService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks whether a CRM Customer ID already exists.
        /// </summary>
        public async Task<bool> IsDuplicateCustomerIdAsync(string crmCustomerId)
        {
            if (string.IsNullOrWhiteSpace(crmCustomerId))
                return false;

            return await _context.Customers
                .AnyAsync(c =>
                    c.CRMCustomerID == crmCustomerId &&
                    !c.IsDeleted);
        }

        /// <summary>
        /// Checks whether an email address is already in use.
        /// </summary>
        public async Task<bool> IsDuplicateEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return await _context.Customers
                .AnyAsync(c =>
                    c.Email == email &&
                    !c.IsDeleted);
        }

        /// <summary>
        /// Checks whether either the CRM ID or Email already exists.
        /// </summary>
        public async Task<bool> HasDuplicateAsync(Customers customer)
        {
            if (customer == null)
                return false;

            return await _context.Customers.AnyAsync(c =>
                !c.IsDeleted &&
                (
                    c.CRMCustomerID == customer.CRMCustomerID ||
                    c.Email == customer.Email
                ));
        }

        /// <summary>
        /// Returns a descriptive duplicate message.
        /// </summary>
        public async Task<string?> GetDuplicateReasonAsync(Customers customer)
        {
            if (customer == null)
                return null;

            if (await IsDuplicateCustomerIdAsync(customer.CRMCustomerID))
                return $"CRM Customer ID '{customer.CRMCustomerID}' already exists.";

            if (await IsDuplicateEmailAsync(customer.Email))
                return $"Email '{customer.Email}' already exists.";

            return null;
        }
    }
}