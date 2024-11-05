using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshHub.Data.Models;

public class Cart
{
    public int CartId { get; set; }
    public int UserId { get; set; }
    public Dictionary<Product, int> ProductQuantities { get; set; } = new Dictionary<Product, int>();
    public DateTime LastUpdated { get; set; }
}

