using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;

namespace Plan_A_Plant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CouponController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        public CouponController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Coupon> CouponList = unitOfWork.Coupon.GetAll().ToList();
            return View(CouponList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Coupon coupons)
        {

            if (ModelState.IsValid)
            {
                var existingcoupon = unitOfWork.Coupon.Get(c => c.Code == coupons.Code);


                if (existingcoupon != null)
                {
                    ModelState.AddModelError("Code", "Coupon code already exists.");
                    return View(coupons); // Return the view with the error message
                }
                unitOfWork.Coupon.Add(coupons);
                unitOfWork.Save();
                TempData["Success"] = "Coupon Created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Coupon coupon = unitOfWork.Coupon.Get(u => u.CouponId == id);
            if (coupon == null)
            {
                return NotFound();
            }
            return View(coupon);
        }

        [HttpPost]
        public IActionResult Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                unitOfWork.Coupon.Update(coupon);
                unitOfWork.Save();
                TempData["Success"] = "Coupon updated successfully";
                return RedirectToAction("Index");
            }
            return View(coupon);
        }


       
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Coupon CouponList = unitOfWork.Coupon.Get(u => u.CouponId == id);
            if (CouponList == null)
            {
                return NotFound();
            }
            return View(CouponList);

        }
        [HttpPost, ActionName("Delete")]

        public IActionResult DeletePost(int? id)
        {
            Coupon CouponList = unitOfWork.Coupon.Get(u => u.CouponId == id);
            if (CouponList == null)
            {
                return NotFound();
            }

            unitOfWork.Coupon.Remove(CouponList);
            unitOfWork.Save();
            TempData["Success"] = "Coupon Deleted successfully";
            return RedirectToAction("Index");
        }

    }
}
