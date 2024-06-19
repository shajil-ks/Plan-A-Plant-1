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
            try
            {
                List<Coupon> CouponList = unitOfWork.Coupon.GetAll().ToList();
                return View(CouponList);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the coupon list. Please try again.";
                 return View(new List<Coupon>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Coupon coupons)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var existingcoupon = unitOfWork.Coupon.Get(c => c.Code == coupons.Code);

                    if (existingcoupon != null)
                    {
                        ModelState.AddModelError("Code", "Coupon code already exists.");
                        return View(coupons); 
                    }

                    unitOfWork.Coupon.Add(coupons);
                    unitOfWork.Save();
                    TempData["Success"] = "Coupon Created successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while creating the coupon. Please try again.";
                
            }

            return View(coupons); 
        }



        public IActionResult Edit(int? id)
        {
            try
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
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the coupon details. Please try again.";
                return RedirectToAction("Index"); 
            }
        }


        [HttpPost]
        public IActionResult Edit(Coupon coupon)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    unitOfWork.Coupon.Update(coupon);
                    unitOfWork.Save();
                    TempData["Success"] = "Coupon updated successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating the coupon. Please try again.";
                
            }

            return View(coupon); 
        }




        public IActionResult Delete(int? id)
        {
            try
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
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the coupon details. Please try again.";
                return RedirectToAction("Index"); 
            }
        }



        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            try
            {
                Coupon coupon = unitOfWork.Coupon.Get(u => u.CouponId == id);
                if (coupon == null)
                {
                    return NotFound();
                }

                unitOfWork.Coupon.Remove(coupon);
                unitOfWork.Save();
                TempData["Success"] = "Coupon Deleted successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {               
                TempData["error"] = "An error occurred while deleting the coupon. Please try again.";
                return RedirectToAction("Index"); 
            }
        }


    }
}
