using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;
using static System.Net.WebRequestMethods;

namespace Plan_A_Plant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM ShoppingCartVM { get; set; }

        public static bool WalletChecked { get; set; }
        public static bool CouponChecked { get; set; }
        public static double CouponDiscountAmount { get; set; }
          

       
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var clamisIdentity = (ClaimsIdentity)User.Identity;
            var userId = clamisIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Product.ProductImages = productImages.Where(u => u.ProductId == cart.ProductId).ToList();
                cart.Price = GetPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }


        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            


            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new(),
                CouponList = _unitOfWork.Coupon.GetAll().ToList(),
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId)
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.MobileNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;

            ShoppingCartVM.OrderHeader.OrderTotal = 0; // Reset the order total before calculating

            // Validate cart items count and product stock
            int totalCartCount = ShoppingCartVM.ShoppingCartList.Sum(c => c.Count);
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                // Check if the product count exceeds the limit per item
                if (cart.Count > 5)
                {
                    // Show temp data for limit exceeded
                    TempData["error"] = $"Limit exceeded: Maximum 5 units of {cart.Product.Name} are allowed.";
                    return RedirectToAction(nameof(Index)); // Redirect to Index or any other action you prefer
                }

                cart.Price = GetPrice(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);

                // Check if cart count exceeds product stock
                if (cart.Count > cart.Product.Qty)
                {
                    // Show temp data for stock over
                    TempData["error"] = $"Stock over: {cart.Product.Name} has insufficient stock.";
                    return RedirectToAction(nameof(Index)); // Redirect to Index or any other action you prefer
                }
            }


            // Populate the dropdown options for previous addresses
            var previousAddresses = _unitOfWork.OrderHeader
                .GetAll(o => o.ApplicationUserId == userId)
                .Select(o => new SelectListItem
                {
                    Text = $"{o.Name}, {o.StreetAddress}, {o.City}, {o.State}, {o.PostalCode}, {o.MobileNumber}",
                    Value = o.StreetAddress // You can set this to any unique identifier of the address if needed
                })
                .Distinct() // Remove duplicates
                .ToList();

            // Pass the error message, if any, to the view
            ViewBag.ErrorMessage = TempData["Message"];

            // Populate the dropdown options
            ViewBag.PreviousAddresses = previousAddresses;

           
            return View(ShoppingCartVM);

        }



        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var UserIdentity = (ClaimsIdentity)User.Identity;
            var UserId = UserIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            // Ensure ShoppingCartVM is initialized
            if (ShoppingCartVM == null)
            {
                ShoppingCartVM = new ShoppingCartVM();
            }

            // Ensure OrderHeader is initialized
            if (ShoppingCartVM.OrderHeader == null)
            {
                ShoppingCartVM.OrderHeader = new OrderHeader();
            }


            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(
                u => u.ApplicationUserId == UserId, includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = UserId;

            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == UserId);
            foreach (var cartItem in ShoppingCartVM.ShoppingCartList)
            {
                cartItem.Price = GetPrice(cartItem);
                ShoppingCartVM.OrderHeader.OrderTotal += (cartItem.Price * cartItem.Count);
            }

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                if (cart.Product != null && cart.Product.Qty > 0)
                {
                    cart.Product.Qty -= cart.Count;
                    if (cart.Product.Qty < 0)
                    {
                        cart.Product.Qty = 0;
                    }
                }
            }

            if (CouponChecked)
            {
                ShoppingCartVM.OrderHeader.OrderTotal = CouponDiscountAmount;
                // Mark the coupon as used for the user
                if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.CouponCode))
                {
                    var usedCoupons = applicationUser.UsedCoupons != null
                        ? applicationUser.UsedCoupons.Split(',').ToList()
                        : new List<string>();

                    if (!usedCoupons.Contains(ShoppingCartVM.OrderHeader.CouponCode))
                    {
                        usedCoupons.Add(ShoppingCartVM.OrderHeader.CouponCode);
                        applicationUser.UsedCoupons = string.Join(",", usedCoupons);
                        _unitOfWork.ApplicationUser.Update(applicationUser);
                        _unitOfWork.Save();
                    }
                }


            }

            //COD Payment
            if (ShoppingCartVM.OrderHeader.PaymentMethod == SD.PaymentMethodCOD.ToString())
            {
                if (ShoppingCartVM.OrderHeader.OrderTotal > 1000)
                {
                    TempData["error"] = "COD is not allowed for orders above Rs 1000.";
                    return RedirectToAction(nameof(Summary));
                }

                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentMethodCODPending;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    OrderDetail orderDetail = new()
                    {
                        Count = cart.Count,
                        Price = cart.Price,
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    };
                    _unitOfWork.OrderDetail.Add(orderDetail);
                    _unitOfWork.Save();
                }
            }

            //Wallet Payment
            if (ShoppingCartVM.OrderHeader.PaymentMethod == SD.PaymentMethodWallet.ToString())
            {
                var userId = ShoppingCartVM.OrderHeader.ApplicationUserId;
                var user = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
                if (user != null && user.Wallet >= ShoppingCartVM.OrderHeader.OrderTotal)
                {
                    user.Wallet -= (int)ShoppingCartVM.OrderHeader.OrderTotal;
                    _unitOfWork.ApplicationUser.Update(user);
                    _unitOfWork.Save();

                    ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusApproved;
                    ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                    _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                    _unitOfWork.Save();
                    foreach (var cart in ShoppingCartVM.ShoppingCartList)
                    {
                        OrderDetail orderDetail = new()
                        {
                            Count = cart.Count,
                            Price = cart.Price,
                            ProductId = cart.ProductId,
                            OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                        };
                        _unitOfWork.OrderDetail.Add(orderDetail);
                        _unitOfWork.Save();
                    }
                }
                else
                {
                    TempData["error"] = "Insufficient amount in your wallet.";
                    return RedirectToAction(nameof(Summary));
                }


            }



            else if (ShoppingCartVM.OrderHeader.PaymentMethod == SD.PaymentMethodOnline.ToString())
            {

                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayPayment;
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;

                _unitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
                _unitOfWork.Save();
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {

                    OrderDetail orderDetail = new()
                    {
                        Count = cart.Count,
                        Price = cart.Price,
                        ProductId = cart.ProductId,
                        OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    };
                    _unitOfWork.OrderDetail.Add(orderDetail);
                    _unitOfWork.Save();

                }

                if (applicationUser != null)
                {
                    decimal totalAmount = (decimal)ShoppingCartVM.OrderHeader.OrderTotal;
                    if (!string.IsNullOrEmpty(ShoppingCartVM.OrderHeader.CouponCode))
                    {
                        var coupon = _unitOfWork.Coupon.Get(u => u.Code == ShoppingCartVM.OrderHeader.CouponCode);
                        totalAmount = CouponCheckOut(ShoppingCartVM.OrderHeader.CouponCode, (int)ShoppingCartVM.OrderHeader.OrderTotal);
                        if (coupon != null)
                        {
                            ShoppingCartVM.OrderHeader.OrderTotal = (double)totalAmount;
                            //coupon.IsValid = SD.CouponInValid; 
                            _unitOfWork.Coupon.Update(coupon);
                            _unitOfWork.Save();
                        }

                    }




                    var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                    var options = new SessionCreateOptions
                    {
                        SuccessUrl = domain + $"user/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                        CancelUrl = domain + "user/cart/Index",
                        LineItems = new List<SessionLineItemOptions>(),
                        Mode = "payment",
                    };

                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long?)(totalAmount * 100),
                            Currency = "INR",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = "Plan A Plant",
                            }
                        },
                        Quantity = 1,
                    };
                    options.LineItems.Add(sessionLineItem);

                    var service = new SessionService();
                    Session session = service.Create(options);
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);
                }
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }




        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader =
                _unitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader.PaymentMethod == SD.PaymentMethodOnline)
            {
                if (orderHeader.PaymentStatus == SD.PaymentStatusDelayPayment)
                {
                    try
                    {
                        var services = new SessionService();
                        Session session = services.Get(orderHeader.SessionId);
                        if (session.PaymentStatus.ToLower() == "paid")
                        {
                            _unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                            _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                            _unitOfWork.Save();
                        }
                        HttpContext.Session.Clear();
                    }
                    catch (StripeException ex)
                    {

                        Console.WriteLine($"Stripe exception: {ex.Message}");
                        return StatusCode(500, "Internal server error while processing the payment.");
                    }

                }
            }


            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);


        }



        public async Task<IActionResult> Coupon(string coupon, int? OrderTotal)
        {
            if (string.IsNullOrEmpty(coupon) || OrderTotal == null)
            {
                return BadRequest();
            }

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            var applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            var couponObj = _unitOfWork.Coupon.Get(u => u.Code == coupon);

            if (couponObj != null)
            {
                if (applicationUser.UsedCoupons != null && applicationUser.UsedCoupons.Split(',').Contains(coupon))
                {
                    TempData["error"] = "You have already used this coupon.";
                    var response = new
                    {
                        success = false,
                        errorMessage = "You have already used this coupon."
                    };
                    return Json(response);
                }

                if (couponObj.IsValid == SD.CouponInValid)
                {
                    TempData["error"] = "The Coupon is Invalid.";
                    var response = new
                    {
                        success = false,
                        errorMessage = "The Coupon is Invalid."
                    };
                    return Json(response);
                }
                else
                {
                    if (couponObj.MinimumAmount <= OrderTotal)
                    {
                        double discountPrice = 0;
                        double cartTotal = (double)OrderTotal;

                        if (couponObj.Type == Models.Coupon.DiscountType.DiscountAmount)
                        {
                            discountPrice = (double)couponObj.DiscountAmount;
                        }
                        else if (couponObj.Type == Models.Coupon.DiscountType.DiscountPercentage)
                        {
                            discountPrice = (double)(cartTotal * couponObj.DiscountPercentage / 100);
                        }

                        double newTotal = cartTotal - discountPrice;

                        var response = new
                        {
                            success = true,
                            discountPrice,
                            newTotal
                        };
                        CouponChecked = true;
                        CouponDiscountAmount = newTotal; // Update CouponDiscountAmount to new total after discount
                        return Json(response);
                    }
                    else
                    {
                        TempData["error"] = "Order total is below the minimum purchase amount.";
                        var response = new
                        {
                            success = false,
                            errorMessage = "Order total is below the minimum purchase amount."
                        };
                        return Json(response);
                    }
                }
            }
            TempData["error"] = "Coupon not found.";
            var responseNotFound = new
            {
                success = false,
                errorMessage = "Coupon not found"
            };
            return Json(responseNotFound);
        }




        private decimal CouponCheckOut(string couponCode, int orderTotal)
        {
            var couponobj = _unitOfWork.Coupon.Get(u => u.Code == couponCode);
            decimal newTotal = (decimal)orderTotal;
            if (couponobj != null)
            {
                if (couponobj.MinimumAmount < orderTotal)
                {
                    if (couponobj.DiscountAmount != null && couponobj.DiscountAmount > 0)
                    {
                        newTotal = (decimal)(orderTotal);
                    }
                    else if (couponobj.DiscountPercentage != null && couponobj.DiscountPercentage > 0)
                    {
                        newTotal = (decimal)(orderTotal);
                    }


                }
            }
            return newTotal;
        }


        public IActionResult CheckWallet(int? totalAmount, string? userId)
        {
            var userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);

            string message = "";
           

            if (userobj.Wallet > totalAmount)
            {
                if (CouponChecked)
                {
                    totalAmount = (int)CouponDiscountAmount;
                }
                var newwalletAmount = userobj.Wallet - totalAmount;
                message = "You may proceed with Wallet Payment";

                var response = new
                {
                    success = true,
                    newWalletAmount = newwalletAmount,
                    message = message,

                };
                WalletChecked = true;
                return Json(response);

            }
            else
            {
                var response = new
                {
                    success = false,
                    newWalletAmount = userobj.Wallet,
                  
                    message = "Insufficient amount in your wallet"

                };

                return Json(response);
            }

        }

        public IActionResult IsNotCheckWallet(int? totalAmount, string? userId)
        {
            var userobj = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
            WalletChecked = false;
            string message = "";
            var response = new
            {
                success = true,
                message = message,

            };

            return Json(response);



        }




        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ShopCartId == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ShopCartId == cartId);
            if (cartFromDb.Count <= 1)
            {
                //remove from cart
                _unitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
            }

            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.ShopCartId == cartId);
            _unitOfWork.ShoppingCart.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        private double GetPrice(ShoppingCart shoppingCart)
        {
            return (double)shoppingCart.Product.DiscountedPrice;
        }
    }
}




