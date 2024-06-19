using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository.IRepository;

namespace Plan_A_Plant.Areas.User.Controllers
{

    [Area("User")]
    public class UserCouponController : Controller
    {
        public IUnitOfWork _unitOfWork;

        public UserCouponController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        [Authorize]
        public IActionResult Index()
        {
            try
            {
                List<Coupon> coupons = _unitOfWork.Coupon.GetAll().ToList();
                return View(coupons);
            }
            catch (Exception ex)
            {
               
                TempData["error"] = "An error occurred while fetching the coupons.";
                return RedirectToAction("Error", "Home");
            }
        }




    }
}
