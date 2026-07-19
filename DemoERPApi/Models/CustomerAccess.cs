using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DemoERPApi.Models;

[Table("CustomerAccess")]
public class CustomerAccess
{
    [Key]
    public int Id { get; set; }


    // External CRM customer identifier (matches Customers.CRMCustomerID)
    public string CRMCustomerID { get; set; } = string.Empty;


    public string Username { get; set; } = string.Empty;


  //  public string Role { get; set; } = string.Empty;


  /// <summary>
  ///  public Customers Customer { get; set; } = null!;
  /// </summary>

    public int UserId { get; set; }





}
