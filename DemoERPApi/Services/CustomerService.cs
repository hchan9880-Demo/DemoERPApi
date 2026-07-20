using DemoERPApi.Data;
using DemoERPApi.Exceptions;
using DemoERPApi.Interfaces;
using DemoERPApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoERPApi.Services;


/// Customer service for CRUD operations, sync, and duplicate detection.

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ILoggingService _loggingService;
    private readonly IDuplicateDetectionService _duplicateDetectionService;

    public CustomerService(
        AppDbContext context,
        IAuditService auditService,
        ILoggingService loggingService,
        IDuplicateDetectionService duplicateDetectionService)
    {
        _context = context;
        _auditService = auditService;
        _loggingService = loggingService;
        _duplicateDetectionService = duplicateDetectionService;
    }

    
    /// Creates or syncs a customer with audit and sync logging.
    
    public async Task<Customers> SyncCustomerAsync(CustomersDto dto, string userName)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ValidationException("Email is required");

        var existing = await _context.Customers
            .FirstOrDefaultAsync(x => x.CRMCustomerID == dto.CRMCustomerID);

        if (existing != null)
            throw new BusinessException("CUSTOMER_DUPLICATE", "Customer already exists");

        var customer = new Customers
        {
            CRMCustomerID = dto.CRMCustomerID,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            LastUpdated = DateTime.UtcNow
        };

        _context.Customers.Add(customer);

        _context.SyncLogs.Add(new SyncLogs
        {
            CRMCustomerID = dto.CRMCustomerID,
            Status = "SUCCESS",
            Message = "Customer created successfully",
            Username = userName,
            CreatedDate = DateTime.UtcNow
        });

        _context.AuditLogs.Add(new AuditLogs
        {
            Action = "CREATE",
            EntityName = "Customer",
            EntityId = dto.CRMCustomerID,
            ChangedDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return customer;
    }

    
    /// Returns all active (non-deleted) customers.
    
    public async Task<IEnumerable<Customers>> GetCustomersAsync()
    {
        return await _context.Customers
            .Where(c => !c.IsDeleted)
            .ToListAsync();
    }

    
    /// Returns a customer by CRM ID. Throws if not found or deleted.
    
    public async Task<Customers?> GetCustomerAsync(string crmCustomerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.CRMCustomerID == crmCustomerId && !x.IsDeleted);

        if (customer == null)
            throw new NotFoundException("Customer not found");

        return customer;
    }

    
    /// Soft-deletes a customer by CRM ID.
    
    public async Task<bool> DeleteCustomerAsync(string crmCustomerId)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(x => x.CRMCustomerID == crmCustomerId);

        if (customer == null)
            throw new NotFoundException("Customer not found");

        customer.IsDeleted = true;
        customer.LastUpdated = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }
}