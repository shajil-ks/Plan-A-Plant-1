using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace Plan_A_Plant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int orderId)
        {
            try
            {
                OrderVM = new OrderVM
                {
                    OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
                };

                if (OrderVM.OrderHeader == null)
                {
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(OrderVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while retrieving the order details. Please try again.";
                
                return RedirectToAction(nameof(Index));
            }
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult StartProcessing()
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
                if (orderHeader == null)
                {
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
                _unitOfWork.Save();

                TempData["success"] = "Order details updated successfully.";
            }
            catch (Exception ex)
            {                
                TempData["error"] = "An error occurred while updating the order status. Please try again.";
               
            }

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }



        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult ShipOrder()
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
                if (orderHeader == null)
                {
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
                orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
                orderHeader.OrderStatus = SD.StatusShipped;
                orderHeader.ShippingDate = DateTime.Now;

                _unitOfWork.OrderHeader.Update(orderHeader);
                _unitOfWork.Save();

                TempData["success"] = "Order shipped successfully.";
            }
            catch (Exception ex)
            {              
                TempData["error"] = "An error occurred while shipping the order. Please try again.";
                
            }

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult Delivered()
        {
            try
            {
                _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusDelivered);
                _unitOfWork.Save();

                TempData["success"] = "Order Delivered successfully.";
            }
            catch (Exception ex)
            {              
                TempData["error"] = "An error occurred while marking the order as delivered. Please try again.";
                
            }

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }





        [HttpPost]
        public IActionResult CancelOrder(int orderId, string reason)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
         
            if (orderHeader != null)
            {
                if (User.IsInRole(SD.Role_User))
                {
                    orderHeader.CancellationReason = reason;
                    orderHeader.CancellationRequestDate = DateTime.Now;
                    orderHeader.CancellationStatus = SD.StatusPending;

                    _unitOfWork.OrderHeader.Update(orderHeader);
                    if (orderHeader.PaymentMethod == SD.PaymentMethodCOD)
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.CancelRequest);
                    }
                    if(orderHeader.PaymentMethod == SD.PaymentMethodWallet  || orderHeader.PaymentMethod == SD.PaymentMethodOnline)
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.CancelRequest, SD.RequestRefund);
                    }
                    
                    _unitOfWork.Save();

                    TempData["success"] = "Cancellation request submitted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                if (User.IsInRole(SD.Role_Admin))
                {
                    if (orderHeader.PaymentMethod == SD.PaymentMethodCOD)
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
                    }
                    if (orderHeader.PaymentMethod == SD.PaymentMethodWallet || orderHeader.PaymentMethod == SD.PaymentMethodOnline)
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
                    }
                    _unitOfWork.Save();

                    var orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id, includeProperties: "Product");

                    // after the order cancellation Increment stock quantity for each product in the order
                    foreach (var orderDetail in orderDetails)
                    {
                        var product = orderDetail.Product;
                        if (product != null)
                        {
                            product.Qty += orderDetail.Count;
                            _unitOfWork.Product.Update(product);
                        }
                    }

                    _unitOfWork.Save();


                    TempData["Success"] = "Order Cancelled  successfully";
                    return RedirectToAction(nameof(Index));
                }



            }

            TempData["error"] = "Order not found.";
            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult ApproveCancellation(int orderId)
        {
            try
            {
                var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
                if (orderHeader != null)
                {
                    orderHeader.CancellationStatus = SD.StatusApproved;
                    _unitOfWork.OrderHeader.Update(orderHeader);

                    if (orderHeader.PaymentMethod == SD.PaymentMethodCOD)
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
                    }
                    else if (orderHeader.PaymentMethod == SD.PaymentMethodWallet || orderHeader.PaymentMethod == SD.PaymentMethodOnline)
                    {
                        _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);

                        // Credit the total amount to the user's wallet for online and wallet payment methods
                        var user = orderHeader.ApplicationUser;
                        if (user != null)
                        {
                            user.Wallet += (int)orderHeader.OrderTotal;
                            _unitOfWork.ApplicationUser.Update(user);
                        }
                    }

                    _unitOfWork.Save();

                    // Increment stock quantity for each product in the order
                    var orderDetails = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderHeader.Id, includeProperties: "Product");
                    foreach (var orderDetail in orderDetails)
                    {
                        var product = orderDetail.Product;
                        if (product != null)
                        {
                            product.Qty += orderDetail.Count;
                            _unitOfWork.Product.Update(product);
                        }
                    }

                    _unitOfWork.Save();

                    TempData["success"] = "Cancellation request approved/Refunded successfully.";
                }
                else
                {
                    TempData["error"] = "Order not found.";
                }
            }
            catch (Exception ex)
            {                
                TempData["error"] = "An error occurred while processing cancellation approval/refund. Please try again.";
                
            }

            return RedirectToAction(nameof(Index));
        }



        [HttpPost]
        public IActionResult ContinuePayment(int orderId)
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            if (orderHeader != null && orderHeader.PaymentStatus == SD.PaymentStatusDelayPayment)
            {
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"user/cart/OrderConfirmation?id={OrderVM.OrderHeader.Id}",
                    CancelUrl = domain + "user/cart/Index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long?)(orderHeader.OrderTotal * 100),
                        Currency = "INR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions()
                        {
                            Name = "Plan A Plant",
                        }
                    },
                    Quantity = 1,
                };
                options.LineItems.Add(sessionLineItem);

                try
                {
                    var service = new SessionService();
                    Session session = service.Create(options);
                    _unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                    TempData["error"] = "An error occurred while processing your payment. Please try again.";
                    return RedirectToAction(nameof(OrderConfirmation), new { id=OrderVM.OrderHeader.Id });
                }
            }

            TempData["error"] = "Invalid order or payment status.";
            return RedirectToAction(nameof(OrderConfirmation), new { id = OrderVM.OrderHeader.Id });
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

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

        public IActionResult Invoice(int orderId)
        {
            try
            {
                OrderVM = new OrderVM
                {
                    OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                    OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
                };

                if (OrderVM.OrderHeader == null)
                {
                    TempData["error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(OrderVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while generating the invoice. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }




        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin))
            {
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeaders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == SD.PaymentStatusDelayPayment);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;
                case "delivered":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == SD.StatusDelivered);
                    break;
                default:
                    break;
            }

            return Json(new { data = objOrderHeaders });
        }
        #endregion
    }
}

