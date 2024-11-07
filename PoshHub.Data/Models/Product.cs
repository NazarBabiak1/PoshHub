using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoshHub.Data.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } // Кількість в наявності
    public string Category { get; set; }
    public string ImageUrl { get; set; } // Посилання на зображення товару
    public ICollection<Review> Reviews { get; set; }
}