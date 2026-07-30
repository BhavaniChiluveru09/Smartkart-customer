using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartKart.Data;
using SmartKart.Models;
using SmartKart.Models.CRM;
using System.Collections;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace SmartKart.Controllers

{
    public class HomeController : Controller
    {
        public static List<User> users = new List<User>();
        public static List<CartItem> cartItems = new List<CartItem>();

        private static List<SupportCaseDto> temporaryCases = new List<SupportCaseDto>();

        private readonly CrmService _crmService;
        private readonly AppDbContext _context;


        public HomeController(AppDbContext context, CrmService crmService)
        {
            _context = context;
            _crmService = crmService;
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult AIAssistant()
        {
            ViewBag.UserName = HttpContext.Session.GetString("UserName");
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string Name, string Email, string Phone, string Password)
        {
            var existingUser = _context.Customers
            .FirstOrDefault(u => u.Email == Email);

            if (existingUser != null)
            {
                TempData["AlreadyRegistered"] = "Account already exists. Please login ";
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(Name) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                ViewBag.Error = "All fields are required";
                return View();
            }

            if (!Email.Contains("@") || !Email.Contains("."))
            {
                ViewBag.Error = "Invalid email format ";
                return View();
            }

            if (!Regex.IsMatch(Password, @"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$"))
            {
                ViewBag.Error = "Password must be at least 8 characters, include 1 uppercase, 1 number, and 1 special character";
                return View();
            }

            if (!Regex.IsMatch(Phone, @"^[6-9]\d{9}$"))
            {
                ViewBag.Error = "Enter a valid 10-digit mobile number starting with 6-9 ";
                return View();
            }

            User newUser = new User
            {
                Name = Name,
                Email = Email,
                Phone = Phone,
                Password = Password
            };

            try
            {
                _context.Customers.Add(newUser);
                _context.SaveChanges();

                // ✅ CRM CALL (new code added)
                _crmService.SendCustomerProfile(new CustomerProfileDto
                {
                    customerName = Name,
                    email = Email,
                    phone = Phone,
                    address = "Not Provided",
                    //preferences = "Default",
                    crmStatus = "Active",
                    userId = newUser.Id.ToString()
                }).Wait(); // sync method ke liye
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message);
                throw;
            }

            return RedirectToAction("Login");
        }

        //[HttpPost]
        //public IActionResult Login(string Email, string Password)
        //{
        //    var user = _context.Customers
        //        .FirstOrDefault(u => u.Email == Email && u.Password == Password);

        //    if (user != null)
        //    {
        //        HttpContext.Session.SetString("UserEmail", user.Email);

        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        ViewBag.Error = "Invalid Email or Password";
        //        return View();
        //    }
        //}
        [HttpPost]
        public IActionResult Login(string Email, string Password)
        {
            // Check if email exists first
            var existingUser = _context.Customers
                .FirstOrDefault(u => u.Email == Email);

            if (existingUser == null)
            {
                ViewBag.Error = "User does not exist. Please register.";
                return View();
            }

            // Check password
            if (existingUser.Password != Password)
            {
                ViewBag.Error = "Incorrect password.";
                return View();
            }

            // Successful login
            HttpContext.Session.SetString("UserEmail", existingUser.Email);

            return RedirectToAction("Index");
        }

        public IActionResult Search(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return RedirectToAction("Index");
            }

            query = query.ToLower();

            if (query.Contains("book"))
                return RedirectToAction("Books");

            if (query.Contains("sport"))
                return RedirectToAction("Sports");

            if (query.Contains("electronic"))
                return RedirectToAction("Electronics");

            if (query.Contains("furniture"))
                return RedirectToAction("Furniture");

            if (query.Contains("accessory"))
                return RedirectToAction("Accessories");

            if (query.Contains("station"))
                return RedirectToAction("Stationery");

            // default fallback
            return RedirectToAction("Index");
        }

        public IActionResult Electronics()
        {

            var products = _context.Products
                .Where(p => p.Category == "Electronics")
                .OrderBy(p => p.Id)
                .ToList();


            return View(products);
        }



        public IActionResult Books()
        {
            var products = _context.Products
                .Where(p => p.Category == "Books")
                .OrderBy(p => p.Id)
                .ToList();

            return View(products);

        }


        public IActionResult Furniture()
        {
            var products = _context.Products
                .Where(p => p.Category == "Furniture")
                .OrderBy(p => p.Id)
                .ToList();

            return View(products);

        }


        public IActionResult Sports()
        {
            var products = _context.Products
                .Where(p => p.Category == "Sports")
                .OrderBy(p => p.Id)
                .ToList();

            return View(products);

        }


        public IActionResult Stationery()
        {
            var products = _context.Products
                .Where(p => p.Category == "Stationery")
                .OrderBy(p => p.Id)
                .ToList();

            return View(products);

        }



        public IActionResult Accessories()
        {
            var products = _context.Products
                .Where(p => p.Category == "Accessories")
                .OrderByDescending(p => p.Id)// temporary
                .ToList();


            return View(products);

        }


        public IActionResult AddToCart(long productId)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var existingItem = _context.Cart
                .FirstOrDefault(c => c.ProductId == productId && c.UserEmail == email);

            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                Cart item = new Cart
                {
                    ProductId = productId,
                    UserEmail = email,
                    Quantity = 1
                };

                _context.Cart.Add(item);
            }

            _context.SaveChanges();

            TempData["CartMessage"] = "Added to cart ✅";


            return Redirect(Request.Headers["Referer"].ToString());


        }


        public IActionResult Cart()
        {

            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                TempData["Error"] = "Please login first";
                return RedirectToAction("Login");
            }



            var cartData = _context.Cart
                .Where(c => c.UserEmail == email)
                .ToList();

            //var cartItems = cartData.Select(c =>
            //{
            //    var product = _context.Products
            //        .FirstOrDefault(p => p.Id == c.ProductId);

            //    return new CartItem
            //    {
            //        CartId = c.Id,
            //        ProductId = product.Id,
            //        Name = product.Name,
            //        Price = product.Price,
            //        Image = product.Image,
            //        Quantity = c.Quantity,
            //        Stock = product.Stock
            //    };
            //}).ToList();

            var cartItems = cartData
    .Select(c =>
    {
        var product = _context.Products
            .FirstOrDefault(p => p.Id == c.ProductId);

        if (product == null)
        {
            return null;
        }

        return new CartItem
        {
            CartId = c.Id,
            ProductId = product.Id,
            Name = product.Name,
            Price = product.Price,
            Image = product.Image,
            Quantity = c.Quantity,
            Stock = product.Stock
        };
    })
    .Where(x => x != null)
    .ToList();

            return View(cartItems);
        }




        public IActionResult OrderSuccess()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Checkout(string address)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                TempData["Error"] = "Address is required";
                return RedirectToAction("Cart");
            }

            var cartItems = _context.Cart
                .Where(c => c.UserEmail == email)
                .ToList();

            if (!cartItems.Any())
            {
                return RedirectToAction("Cart");
            }
            string orderCode = GenerateOrderCode();
            foreach (var item in cartItems)
            {
                Order order = new Order
                {
                    UserEmail = email,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Address = address,
                    OrderDate = DateTime.UtcNow,
                    OrderCode = orderCode // ✅ NEW
                };

                _context.Orders.Add(order);

                //OrderCode for UI
                TempData["OrderCode"] = orderCode;



             

                try
                {
                    _crmService.SendOrder(new OrderDto
                    {
                        orderId = orderCode, // ✅ SAME CODE
                        email = email,
                        address = address,
                        orderDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        totalAmount = item.Quantity * 100,
                        orderStatus = "Placed",
                        userId = _context.Customers
                                   .FirstOrDefault(u => u.Email == email)?.Id.ToString()
                    }).Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("CRM Error: " + ex.Message);
                }

                var product = _context.Products
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (product != null)
                {
                    product.Stock -= item.Quantity;

                    if (product.Stock < 0)
                    {
                        product.Stock = 0;
                    }

                    _context.Products.Update(product);
                }
            }



            // ✅ Save all orders
            _context.SaveChanges();

            // ✅ ✅ CRM UPDATE BLOCK (NEW CODE - SAFE)
            var user = _context.Customers
           .FirstOrDefault(u => u.Email == email);

            if (user != null)
            {
                _crmService
                    .UpdateCustomerAddress(
                        user.Id.ToString(),
                        address
                    )
                    .Wait();
            }
            // ✅ ✅ END CRM BLOCK

            // ✅ Clear user's cart
            _context.Cart.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("OrderSuccess");
        }

        public IActionResult IncreaseQty(long cartId)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var cartItem = _context.Cart
                .FirstOrDefault(c => c.Id == cartId && c.UserEmail == email);

            if (cartItem != null)
            {
                var product = _context.Products
                    .FirstOrDefault(p => p.Id == cartItem.ProductId);

                if (product != null && cartItem.Quantity < product.Stock)
                {
                    cartItem.Quantity++;
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Cart");
        }

        public IActionResult DecreaseQty(long cartId)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var cartItem = _context.Cart
                .FirstOrDefault(c => c.Id == cartId && c.UserEmail == email);

            if (cartItem != null)
            {
                cartItem.Quantity--;

                if (cartItem.Quantity <= 0)
                {
                    _context.Cart.Remove(cartItem);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Cart");
        }

        //public IActionResult ProductDetails(long id)
        //{
        //    var product = _context.Products
        //        .FirstOrDefault(p => p.Id == id);

        //    if (product == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(product);
        //}

        //public IActionResult ProductDetails(long id)
        //{
        //    var product = _context.Products
        //        .FirstOrDefault(p => p.Id == id);

        //    if (product == null)
        //    {
        //        return Content("Product not found. Id = " + id);
        //    }

        //    return View(product);
        //}

        public IActionResult ProductDetails(long id)
        {
            // 1. Get current product
            var product = _context.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return Content("Product not found. Id = " + id);
            }

            // 2. Find all OrderCodes where this product was purchased
            var relatedOrderCodes = _context.Orders
                .Where(o => o.ProductId == id && o.OrderCode != null)
                .Select(o => o.OrderCode)
                .Distinct()
                .ToList();

            // 3. Find other products bought in those same orders
            var frequentlyBoughtIds = _context.Orders
                .Where(o => relatedOrderCodes.Contains(o.OrderCode)
                            && o.ProductId != id)
                .GroupBy(o => o.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(4)
                .ToList();

            // 4. Fetch recommended products from Products table
            var recommendedProducts = new List<Product>();

            foreach (var item in frequentlyBoughtIds)
            {
                var recommendedProduct = _context.Products
                    .FirstOrDefault(p => p.Id == item.ProductId);

                if (recommendedProduct != null)
                {
                    recommendedProducts.Add(recommendedProduct);
                }
            }

            // 5. Fallback: if frequently bought products are less than 4,
            // add products from same category
            //if (recommendedProducts.Count < 4)
            //{
            //    var existingRecommendedIds = recommendedProducts
            //        .Select(p => p.Id)
            //        .ToList();

            //    existingRecommendedIds.Add(product.Id);

            //    var fallbackProducts = _context.Products
            //        .Where(p => p.Category == product.Category
            //                    && !existingRecommendedIds.Contains(p.Id))
            //        .OrderBy(p => p.Id)
            //        .Take(4 - recommendedProducts.Count)
            //        .ToList();

            //    recommendedProducts.AddRange(fallbackProducts);
            //}

            // Step 1 : Same SubCategory

            if (recommendedProducts.Count < 4)
            {
                var existingRecommendedIds = recommendedProducts
                    .Select(p => p.Id)
                    .ToList();

                existingRecommendedIds.Add(product.Id);

                var subCategoryProducts = _context.Products
                    .Where(p => p.SubCategory == product.SubCategory
                                && !existingRecommendedIds.Contains(p.Id))
                    .Take(4 - recommendedProducts.Count)
                    .ToList();

                recommendedProducts.AddRange(subCategoryProducts);
            }

            // Step 2 : Same Category

            if (recommendedProducts.Count < 4)
            {
                var existingRecommendedIds = recommendedProducts
                    .Select(p => p.Id)
                    .ToList();

                existingRecommendedIds.Add(product.Id);

                var categoryProducts = _context.Products
                    .Where(p => p.Category == product.Category
                                && !existingRecommendedIds.Contains(p.Id))
                    .Take(4 - recommendedProducts.Count)
                    .ToList();

                recommendedProducts.AddRange(categoryProducts);
            }

            string recommendationTitle;

            if (frequentlyBoughtIds.Any())
            {
                recommendationTitle = "Frequently Bought Together";
            }
            else
            {
                recommendationTitle = "You May Also Like";
            }

            // 6. Send product + recommendations to the view
            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                RecommendedProducts = recommendedProducts,
                RecommendationTitle = recommendationTitle
            };

            return View(viewModel);
        }

        public IActionResult Orders()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var orders = _context.Orders
                .Where(o => o.UserEmail == email)
                .ToList();

            var currentTime = DateTime.UtcNow;
            bool hasStatusChanges = false;

            // ✅ Auto status update + DB save logic
            foreach (var order in orders)
            {
                string currentStatus = string.IsNullOrWhiteSpace(order.OrderStatus)
                    ? "Created"
                    : order.OrderStatus;

                // ✅ Cancelled / Refunded orders ko auto update nahi karna
                if (!order.IsCancelled &&
                    !order.IsRefunded &&
                    currentStatus != "Cancelled" &&
                    currentStatus != "Refunded")
                {
                    double minutesPassed = (currentTime - order.OrderDate).TotalMinutes;

                    string newStatus = currentStatus;

                    // ✅ Deep fix: direct time ke base pe final status decide
                    if (minutesPassed >= 2)
                    {
                        newStatus = "Delivered";
                    }
                    else if (minutesPassed >= 1)
                    {
                        newStatus = "Packed";
                    }
                    else
                    {
                        newStatus = "Created";
                    }

                    // ✅ DB me update only if changed
                    if (order.OrderStatus != newStatus)
                    {
                        order.OrderStatus = newStatus;
                        hasStatusChanges = true;
                    }
                }
            }

            // ✅ Save updated statuses in database
            if (hasStatusChanges)
            {
                _context.SaveChanges();
            }

            var orderItems = orders.Select(o =>
            {
                var product = _context.Products
                    .FirstOrDefault(p => p.Id == o.ProductId);

                return new
                {
                    o.Id,
                    o.OrderCode,
                    ProductName = product?.Name,
                    Price = product?.Price,
                    Image = product?.Image,
                    o.Quantity,
                    o.Address,
                    o.OrderDate,

                    // ✅ DB se latest status
                    OrderStatus = o.OrderStatus ?? "Created",

                    o.IsCancelled,
                    o.IsRefunded
                };
            }).ToList();

            return View(orderItems);
        }



        //temporary to check session
        public IActionResult CheckSession()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return Content("User NOT logged in");
            }

            return Content("Logged in user: " + email + " ✅");
        }

        //Profile
        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Customers
                .FirstOrDefault(u => u.Email == email);

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // ✅ remove all session data

            TempData["Message"] = "Logged out successfully ✅";

            return RedirectToAction("Index");
        }


        //Support

        public IActionResult Support()
        {
            return View();
        }

        public IActionResult RaiseCase()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Customers.FirstOrDefault(u => u.Email == email);

            var allOrders = _context.Orders
                .Where(o => o.UserEmail == email)
                .Select(o => o.OrderCode)
                .Distinct()
                .ToList();

            // ✅ Filter out orders already having cases
            var availableOrders = new List<string>();

            foreach (var oid in allOrders)
            {
                bool alreadyCaseRaised = temporaryCases.Any(c =>
                    c.userId == user.Id.ToString() &&
                    c.orderId == oid
                );

                if (!alreadyCaseRaised)
                {
                    availableOrders.Add(oid);
                }
            }

            ViewBag.OrderIds = availableOrders;

            return View();
        }






        [HttpPost]
        public IActionResult RaiseCase(string caseType, string orderId, string description)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Customers.FirstOrDefault(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "User not found. Please login again.";
                return RedirectToAction("RaiseCase");
            }

            // ✅ Basic validation
            if (string.IsNullOrWhiteSpace(caseType) ||
                string.IsNullOrWhiteSpace(orderId) ||
                string.IsNullOrWhiteSpace(description))
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("RaiseCase");
            }

            // ✅ Check: entered Order ID belongs to logged-in customer
            bool validOrder = _context.Orders.Any(o =>
                o.OrderCode == orderId &&
                o.UserEmail == email
            );

            if (!validOrder)
            {
                TempData["Error"] = "Invalid Order ID.";
                return RedirectToAction("RaiseCase");
            }

            // ✅ DB duplicate check (FINAL permanent fix)
            bool alreadyInDb = _context.SupportCases.Any(c =>
                c.OrderId == orderId &&
                c.UserId == user.Id.ToString()
            );

            if (alreadyInDb)
            {
                TempData["Error"] = "You have already raised a case for this Order ID.";
                return RedirectToAction("RaiseCase");
            }

            // ✅ Create DB object
            var dbCase = new SupportCase
            {
                CaseTitle = description,
                CaseType = caseType,
                OrderId = orderId,
                UserId = user.Id.ToString(),
                UserEmail = email,
                Priority = "Medium",
                Status = "Open",
                Resolution = "",
                Owner = "Support Team",
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Save in DB
            _context.SupportCases.Add(dbCase);
            _context.SaveChanges();

            // ✅ CRM object (same as before)
            var crmCase = new SupportCaseDto
            {
                caseTitle = description,
                caseType = caseType,
                orderId = orderId,
                userId = user.Id.ToString(),
                priority = "Medium",
                status = "Open",
                resolution = "",
                owner = "Support Team"
            };

            _crmService.SendSupportCase(crmCase).Wait();

            TempData["Success"] = "Case submitted successfully!";
            return RedirectToAction("Support");
        }


       

        public IActionResult MyCases()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
            {
                return RedirectToAction("Login");
            }

            var cases = _context.SupportCases
                .Where(c => c.UserEmail == email)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            bool isStatusUpdated = false;

            // ✅ CRM se latest status fetch + DB update
            foreach (var item in cases)
            {
                var crmStatus = _crmService
                    .GetSupportCaseStatusByOrderId(item.OrderId)
                    .Result;

                if (!string.IsNullOrWhiteSpace(crmStatus))
                {
                    // ✅ Agar CRM status DB status se different hai tabhi update
                    if (item.Status != crmStatus)
                    {
                        item.Status = crmStatus;
                        isStatusUpdated = true;
                    }
                }
            }

            // ✅ Database me status save
            if (isStatusUpdated)
            {
                _context.SaveChanges();
            }

            return View(cases);
        }

        // Order code method
        private string GenerateOrderCode()
        {
            Random random = new Random();

            // 3 random uppercase letters
            string letters = "";
            for (int i = 0; i < 3; i++)
            {
                letters += (char)random.Next('A', 'Z' + 1);
            }

            // 6 random digits
            string numbers = random.Next(100000, 999999).ToString();

            return letters + numbers;
        }

        [HttpPost]
        public IActionResult CancelOrder(string orderCode)
        {
            var orders = _context.Orders
                .Where(o => o.OrderCode == orderCode)
                .ToList();

            if (orders.Any())
            {
                // ✅ Agar already cancelled/refunded hai to dobara stock add nahi karna
                bool alreadyProcessed = orders.Any(o => o.IsCancelled || o.IsRefunded);

                if (!alreadyProcessed)
                {
                    // ✅ Cancel allowed only before Delivered
                    bool canCancel = orders.All(o => o.OrderStatus != "Delivered");

                    if (canCancel)
                    {
                        foreach (var order in orders)
                        {
                            order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);
                            // ✅ Product stock restore
                            var product = _context.Products
                                .FirstOrDefault(p => p.Id == order.ProductId);

                            if (product != null)
                            {
                                product.Stock += order.Quantity;
                                _context.Products.Update(product);
                            }

                            // ✅ Order status update
                            order.IsCancelled = true;
                            order.OrderStatus = "Cancelled";

                            _context.Orders.Update(order);
                        }

                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Orders");
        }

        [HttpPost]
        public IActionResult RefundOrder(string orderCode)
        {
            var orders = _context.Orders
                .Where(o => o.OrderCode == orderCode)
                .ToList();

            if (orders.Any())
            {
                // ✅ Agar already cancelled/refunded hai to dobara stock add nahi karna
                bool alreadyProcessed = orders.Any(o => o.IsCancelled || o.IsRefunded);

                if (!alreadyProcessed)
                {
                    // ✅ Refund allowed only after Delivered
                    bool canRefund = orders.All(o => o.OrderStatus == "Delivered");

                    if (canRefund)
                    {
                        foreach (var order in orders)
                        {
                            // ✅ Fix PostgreSQL DateTime UTC issue
                            order.OrderDate = DateTime.SpecifyKind(order.OrderDate, DateTimeKind.Utc);

                            // ✅ Product stock restore
                            var product = _context.Products
                                .FirstOrDefault(p => p.Id == order.ProductId);

                            if (product != null)
                            {
                                product.Stock += order.Quantity;
                                _context.Products.Update(product);
                            }

                            // ✅ Order refund update
                            order.IsRefunded = true;
                            order.OrderStatus = "Refunded";

                            _context.Orders.Update(order);
                        }

                        _context.SaveChanges();
                    }
                }
            }

            return RedirectToAction("Orders");
        }
    }
}