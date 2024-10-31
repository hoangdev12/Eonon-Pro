using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBTL.Models
{
    public class CartItem
    {
        public int orderDetailId { get; set; }
        public int orderID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public Nullable<decimal> UnitPrice { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice => (decimal)(UnitPrice * Quantity);
        public string thumb {  get; set; }
    }
}