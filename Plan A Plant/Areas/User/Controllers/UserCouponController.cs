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

            List<Coupon> coupon = _unitOfWork.Coupon.GetAll().ToList();
            return View(coupon);
        }



    }
}
