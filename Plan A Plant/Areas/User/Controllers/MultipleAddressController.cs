using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository.IRepository;
using System.Security.Claims;

namespace Plan_A_Plant.Areas.User.Controllers
{
    [Area("User")]
    public class MultipleAddressController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public MultipleAddress multipleAddress { get; set; }
        public MultipleAddressController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                List<MultipleAddress> addresses = _unitOfWork.MultipleAddress.GetAll(u => u.ApplicationUserId == userId).ToList();

                return View(addresses);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching addresses.";
                return RedirectToAction("Error", "Home");
            }

        }
        public IActionResult AddAddress()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public IActionResult AddAddress(MultipleAddress multipleAddress)
        {
            try
            {
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var claimsIdentity = (ClaimsIdentity)User.Identity;
                    var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                    multipleAddress.Options = 1;
                    multipleAddress.ApplicationUserId = userId;
                    if (ModelState.IsValid)
                    {
                        _unitOfWork.MultipleAddress.Add(multipleAddress);
                        _unitOfWork.Save();

                        TempData["success"] = " Address Created SuccessFully";
                        return RedirectToAction(nameof(Index));
                    }

                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while adding the address.";
            }
            return View(multipleAddress);


        }
        public IActionResult Delete(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }
                MultipleAddress address = _unitOfWork.MultipleAddress.Get(u => u.Id == id);

                return View(address);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the address.";
                return RedirectToAction(nameof(Index));
            }

        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            try
            {
                MultipleAddress address = _unitOfWork.MultipleAddress.Get(u => u.Id == id);
                if (address != null)
                {
                    _unitOfWork.MultipleAddress.Remove(address);
                    _unitOfWork.Save();
                    TempData["success"] = "Address delete Successfully";
                    return RedirectToAction(nameof(Index));
                }
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the address.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
