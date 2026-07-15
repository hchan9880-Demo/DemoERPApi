using DemoERPApi.Data;
using DemoERPApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace DemoERPApi.Controllers;


/*
===============================================================================

 Audit Logs API

 Purpose:

    Provides access to historical data change records.

 Features:

    - View audit history
    - Track who changed data
    - Track what changed
    - Support investigation and compliance

 Endpoint:

    GET /api/AuditLogs

===============================================================================
*/


[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{

    private readonly AppDbContext _context;


    public AuditLogsController(
        AppDbContext context)
    {
        _context = context;
    }



    /*
    ===========================================================================
    
    GET /api/AuditLogs

    Returns all audit records.

    Example:

    [
       {
          "auditId":1,
          "entityName":"Customer",
          "entityId":"CRM100",
          "action":"CREATE",
          "changedBy":"admin",
          "changedDate":"2026-07-15"
       }
    ]

    ===========================================================================
    */


    [HttpGet]
    public async Task<IActionResult> GetAuditLogs()
    {

        var logs =
            await _context.AuditLogs
            .OrderByDescending(x => x.ChangedDate)
            .Select(x => new
            {
                x.AuditId,

                x.EntityName,

                x.EntityId,

                x.Action,

                x.ChangedBy,

                x.RequestId,

                x.ChangedDate
            })
            .ToListAsync();


        return Ok(logs);
    }




    /*
    ===========================================================================
    
    GET /api/AuditLogs/{id}

    Returns single audit record.

    Example:

        /api/AuditLogs/1

    ===========================================================================
    */


    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuditLog(
        int id)
    {

        var audit =
            await _context.AuditLogs
            .FirstOrDefaultAsync(x =>
                x.AuditId == id);


        if (audit == null)
        {
            return NotFound();
        }


        return Ok(audit);
    }

}