using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBTL.Models;
using System.Data.Entity;


namespace WebBTL.Controllers
{
    public class CartController : Controller
    {

        private readonly Eonon_ProEntities1 _context;

        public CartController()
        {
            _context = new Eonon_ProEntities1();
        }

        public ActionResult AddToCart(int productId, int quantity)
        {
            // Tìm sản phẩm từ bảng Product
            var product = _context.Products.FirstOrDefault(p => p.ProductID == productId);

            if (product != null)
            {
                var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

                
                var existingItem = cart.FirstOrDefault(c => c.ProductID == productId);

                if (existingItem != null)
                {
                    
                    existingItem.Quantity += quantity;
                }
                else
                {
                    // Thêm sản phẩm mới vào giỏ hàng
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,  
                        UnitPrice = product.Price,  
                        Quantity = quantity,
                        thumb = product.Thumb
                        
                    });
                }

                // Lưu giỏ hàng vào Session
                Session["Cart"] = cart;

                TempData["SuccessMessage"] = $"{product.ProductName} đã được thêm vào giỏ hàng!";
            }

            if (Session["Email"] == null)
            {
                // Lưu lại URL của trang giỏ hàng vào Session trước khi chuyển hướng
                Session["ReturnUrl"] = Url.Action("Cart");
                return RedirectToAction("Login", "Home");
            } else
            {
                return RedirectToAction("Cart");
            }
            
        }

        public ActionResult Cart()
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();
            return View(cart);
        }
     

        public ActionResult RemoveFromCart(int productId)
        {
            var cart = Session["Cart"] as List<CartItem> ?? new List<CartItem>();

            var itemToRemove = cart.FirstOrDefault(c => c.ProductID == productId);

            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
            }
           
            Session["Cart"] = cart;

            return RedirectToAction("Cart");
        }


        [HttpPost]
        public ActionResult UpdateQuantities(Dictionary<int, int> quantities, int[] selectedProducts)
        {
            var cart = Session["Cart"] as List<CartItem>; 
            foreach (var productId in selectedProducts)
            {
                if (quantities.ContainsKey(productId))
                {
                    var item = cart.FirstOrDefault(i => i.ProductID == productId);
                    if (item != null)
                    {
                        item.Quantity = quantities[productId];
                      
                    }
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails) 
                .Include("OrderDetails.Product") 
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            var cart = Session["Cart"] as List<CartItem>;

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm vào giỏ trước khi thanh toán.";
                return RedirectToAction("Cart");
            }

            if (Session["Email"] == null)
            {
                Session["ReturnUrl"] = Url.Action("Checkout");
                return RedirectToAction("Login", "Home");
            }

            // Lấy thông tin khách hàng dựa trên email từ session
            string email = Session["Email"].ToString();
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản khách hàng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Home");
            }

            // Nếu tất cả điều kiện đều thoả mãn, hiển thị trang thanh toán
            return View(cart); // Pass the cart to the view
        }

        [HttpPost]
        public ActionResult Checkout(Order order)
        {
            var cart = Session["Cart"] as List<CartItem>;

            if (cart == null || !cart.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng thêm sản phẩm vào giỏ trước khi thanh toán.";
                return RedirectToAction("Cart");
            }

            if (Session["Email"] == null)
            {
                Session["ReturnUrl"] = Url.Action("Checkout");
                return RedirectToAction("Login", "Home");
            }

            // Lấy thông tin khách hàng dựa trên email từ session
            string email = Session["Email"].ToString();
            var customer = _context.Customers.FirstOrDefault(c => c.Email == email);

            if (customer == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tài khoản khách hàng. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Home");
            }
       
            // Tạo đơn hàng
            order.CustomerID = customer.CustomerID;
            order.Orderdate = DateTime.Now;
            order.TotalAmount = cart.Sum(c => c.Quantity * c.UnitPrice);

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Tạo chi tiết đơn hàng
            foreach (var item in cart)
            {
                item.orderID = order.OrderID;
                item.orderDetailId++;

                var orderDetail = new OrderDetail
                {                         
                   
                    OrderID = item.orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    Price = item.UnitPrice
                };

                _context.OrderDetails.Add(orderDetail);
            }

            _context.SaveChanges();

            // Xóa giỏ hàng
            Session["Cart"] = null;

            TempData["SuccessMessage"] = "Đơn hàng của bạn đã được đặt thành công!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderID });
        }

    }
}