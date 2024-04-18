using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;
using System.Diagnostics;
using System.Security.Claims;

namespace Plan_A_Plant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

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
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };

            return View(orderVM);
        }

        //[HttpPost]
        //[Authorize(Roles=SD.Role_Admin)]
        //public IActionResult StartProcessing()
        //{
        //    _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
        //    _unitOfWork.Save();
        //    TempData["Success"] = "Order Details Updated Successfully";
        //    return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        //}




        //[HttpPost]
        //public IActionResult CancelOrder()
        //{
        //    var orderHeader = context.OrderHeader.Get(u => u.OrderHeaderId == orderViewModel.OrderHeader.OrderHeaderId);

        //    context.OrderHeader.UpdateStatus(orderHeader.OrderHeaderId, StatDetails.StatusCancelled, StatDetails.StatusCancelled);
        //    context.Save();

        //    var orderDetails = context.OrderDetails.GetAll(u => u.OrderHeader_ID == orderHeader.OrderHeaderId, includeProperties: "Product");

        //    // Increment stock quantity for each product in the order
        //    foreach (var orderDetail in orderDetails)
        //    {
        //        var product = orderDetail.Product;
        //        if (product != null)
        //        {
        //            product.StockQuantity += orderDetail.Count;
        //            context.Product.Update(product);
        //        }
        //    }

        //    context.Save();


        //    TempData["Success"] = "Order Cancelled  successfully";
        //    return RedirectToAction(nameof(Index));

        //}







        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeaders;

            if (User.IsInRole(SD.Role_Admin))
            {
                objOrderHeaders= _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var clamisIdentity = (ClaimsIdentity)User.Identity;
                var userId = clamisIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
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
                default:
                    
                    break;
            }



            return Json(new { data = objOrderHeaders });
        }

        

        #endregion




    }
}
