using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace Plan_A_Plant.Areas.User.Controllers
{
    [Area("User")]
   
    public class HomeController : Controller
    {
      
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private static int walletAmount;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;   
        }

        public IActionResult Index()
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {

                if (User.IsInRole("Admin"))
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                }

            }
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            return View(productList);


        }

        
        public IActionResult Search(string searchString, int? categoryId)
        {
            // Get all products
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            // Filter by category if provided
            if (categoryId != null)
            {
                productList = productList.Where(p => p.CategoryId == categoryId);
            }

            // Filter by search string if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower(); // Convert search string to lowercase for case-insensitive comparison

                // Filter by product name or category name containing the search string
                productList = productList.Where(p =>
                    p.Name.ToLower().Contains(searchString) ||
                    p.Category.Name.ToLower().Contains(searchString));
            }

            var categories = _unitOfWork.Category.GetAll().ToList();
            ViewBag.Categories = categories; // Pass categories to the layout

            return View("Index", productList.ToList());
        }



        public IActionResult ViewProduct(int Id )
        {
            ShoppingCart cart = new()
            {
                  Product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "Category,ProductImages"),
                  Count=1,
                  ProductId=Id
            };
            
            return View(cart);
        }


        [HttpPost]
        [Authorize]
        public IActionResult ViewProduct(ShoppingCart shoppingCart)
        {
            
            //var clamisIdentity=(ClaimsIdentity)User.Identity;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            shoppingCart.ApplicationUserId = userId;

            // Retrieve the shopping cart item from the database
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            // Retrieve the product from the database
            var product = _unitOfWork.Product.Get(p => p.Id == shoppingCart.ProductId);

            // Check if the count is less than or equal to zero
            if (shoppingCart.Count <= 0)
            {
                TempData["Error"] = "Minimum should be one!!!";
                return RedirectToAction(nameof(ViewProduct));
            }

            // Check if the count exceeds the product quantity
            if (shoppingCart.Count > product.Qty)
            {
                TempData["error"] = $"Stock over: {product.Name} has insufficient stock.";
                return RedirectToAction(nameof(ViewProduct));
            }

            // Check if the count exceeds the limit
            int limit = 5;
            if (shoppingCart.Count > limit)
            {
                TempData["error"] = "Limit exceeded: Maximum 5 items allowed in the cart.";
                return RedirectToAction(nameof(ViewProduct));
            }



            if (cartFromDb != null) 
            {  //shopping carexisist
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart. Update(cartFromDb);
                

            }
            else
            {  //create newrecord
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                
            }
            
            _unitOfWork.Save();
            TempData["Success"] = "Cart Updated successfully";
            return RedirectToAction (nameof(Index));
            
        }

        [Authorize]
        public IActionResult UserProfile()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser Userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
            if (Userobj != null)
            {
                return View(Userobj);
            }
            return View();
        }
        public IActionResult EditUserProfile()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ApplicationUser Userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
            if (Userobj != null)
            {
                return View(Userobj);
            }
            return View();
        }

        [HttpPost]
        public IActionResult EditUserProfile(ApplicationUser applicationUser)
        {

            _unitOfWork.ApplicationUser.Update(applicationUser);
            _unitOfWork.Save();
            return RedirectToAction(nameof(UserProfile));
        }  

            



        public IActionResult Wallet()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
            if (userobj.Wallet == null)
            {
                userobj.Wallet = 0;
            }
            if (userobj.Wallet <= 0)
            {
                userobj.Wallet = 0;
            }
            return View(userobj);
        }


        [HttpPost]
        public IActionResult Wallet(ApplicationUser applicationUser)
        {   
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            SetWalletValue((int)applicationUser.Wallet);

            var UserObj = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);

            if (UserObj != null)
            {
                if (UserObj.Wallet == null)
                {
                    UserObj.Wallet = 0;
                }
                TempData["WalletAmount"] = applicationUser.Wallet;
                if (applicationUser.Wallet != null)
                {
                    if (applicationUser.Wallet <= 0)
                    {
                        TempData["ValueNot"] = "Please enter a value greater than 0.";
                        return View(applicationUser);
                    }
                    else
                    {
                        try
                        {
                            var domain = "https://localhost:7208/";
                            var options = new SessionCreateOptions
                            {
                                SuccessUrl = domain + $"user/Home/WalletSuccess?id={UserObj.Id}",
                                CancelUrl = domain + "User/Home/Wallet",
                                LineItems = new List<SessionLineItemOptions>(),
                                Mode = "payment",
                            };

                            var sessionLineItem = new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    UnitAmount = (long?)(applicationUser.Wallet * 100),
                                    Currency = "INR",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = "Plan A Plant",
                                        Description = "Add amount in your wallet"
                                    }
                                },
                                Quantity = 1,
                            };

                            options.LineItems.Add(sessionLineItem);
                            var service = new SessionService();
                            Session session = service.Create(options);

                            // Store the session ID and payment intent ID in the database
                            UserObj.WalletSessionId = session.Id;
                            UserObj.WalletPaymentIntentId = session.PaymentIntentId;
                            _unitOfWork.ApplicationUser.Update(UserObj);
                            _unitOfWork.Save();

                            
                            Response.Headers.Add("Location", session.Url);
                            return new StatusCodeResult(303);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
            }

           
            return View(applicationUser);
        }



        public IActionResult WalletSuccess(string id)
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            var UserId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var UserObj = _unitOfWork.ApplicationUser.Get(u => u.Id == id);

            if (UserObj != null)
            {
                // Check if the payment is successful
                var service = new SessionService();
                Session session = service.Get(UserObj.WalletSessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    // Update wallet amount
                    int walletAmount = Convert.ToInt32(TempData["WalletAmount"]);
                    UserObj.Wallet += walletAmount;
                    _unitOfWork.ApplicationUser.Update(UserObj);
                    _unitOfWork.Save();

                    // Clear session ID and payment intent ID
                    UserObj.WalletSessionId = null;
                    UserObj.WalletPaymentIntentId = null;
                    _unitOfWork.ApplicationUser.Update(UserObj);
                    _unitOfWork.Save();

                    TempData["success"] = "Wallet amount updated successfully.";
                }
                else
                {
                    TempData["error"] = "Payment failed. Wallet amount not updated.";
                }
            }
            else
            {
                TempData["error"] = "User not found.";
            }

            return RedirectToAction(nameof(UserProfile));
        }



        private void SetWalletValue(int amount)
        {
            walletAmount = amount;
        }
        private int GetWalletAmount()
        {
            return walletAmount;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]



        public IActionResult Test()  
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
