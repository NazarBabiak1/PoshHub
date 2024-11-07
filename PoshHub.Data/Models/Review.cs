using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshHub.Data.Models;

public class Review
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public int Rating { get; set; } // Оцінка (наприклад, від 1 до 5)
    public string Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
