using DemoERPApi.Data;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;
using DemoERPApi.Interfaces;

namespace DemoERPApi.Services;


/// Service for detecting duplicate customers by CRM ID and email.

public class DuplicateDetectionService : IDuplicateDetectionService
{
    private readonly AppDbContext _context;

    public DuplicateDetectionService(AppDbContext context)
    {
        _context = context;
    }

    
    /// Checks if a CRM Customer ID already exists (active customers only).
    
    public async Task<bool> IsDuplicateCustomerIdAsync(string crmCustomerId)
    {
        if (string.IsNullOrWhiteSpace(crmCustomerId))
            return false;

        return await _context.Customers
            .AnyAsync(c => c.CRMCustomerID == crmCustomerId && !c.IsDeleted);
    }

    
    /// Checks if an email is already in use (active customers only).
    
    public async Task<bool> IsDuplicateEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return await _context.Customers
            .AnyAsync(c => c.Email == email && !c.IsDeleted);
    }

    
    /// Checks if either CRM ID or Email is a duplicate.
    
    public async Task<bool> HasDuplicateAsync(Customers customer)
    {
        if (customer == null)
            return false;

        return await _context.Customers.AnyAsync(c =>
            !c.IsDeleted &&
            (c.CRMCustomerID == customer.CRMCustomerID ||
             c.Email == customer.Email));
    }

    
    /// Returns a descriptive duplicate reason message.
    
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