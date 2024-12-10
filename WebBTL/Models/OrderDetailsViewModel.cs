using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBTL.Models
{
    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}