using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshHub.Data.Models;

public class User
{
    public int UserId { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; } 
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; } 
    public DateTime CreatedAt { get; set; }
    public Cart CartField { get; set; }
    public List<Order> Orders { get; set; }
    public List<Review> Reviews { get; set; }
}
