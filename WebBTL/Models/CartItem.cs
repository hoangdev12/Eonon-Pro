using System;

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
        public string thumb { get; set; }

        // Use a default value if UnitPrice is null
        public decimal TotalPrice
        {
            get
            {
                // If UnitPrice is null, return 0, otherwise calculate the total price
                return (UnitPrice ?? 0) * Quantity;
            }
        }
    }
}
