using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshHub.Data.Models;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public ICollection<CartItem> CartItems { get; set; }
    public DateTime LastUpdated { get; set; }
}
