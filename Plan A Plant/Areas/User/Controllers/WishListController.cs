using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository.IRepository;
using System.Security.Claims;

namespace Plan_A_Plant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize]
    public class WishListController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        [BindProperty]
        public WishListVM WishList { get; set; }

        public WishListController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var WishlistItems = unitOfWork.WishList.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product.ProductImages"
            );

            return View(WishlistItems);
        }

        [HttpPost]
        public JsonResult AddToWishlist(int productId)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var existingWishlistItem = unitOfWork.WishList.Get(u => u.ApplicationUserId == claim.Value && u.ProductId == productId);
            Product product = unitOfWork.Product.Get(p => p.Id == productId);

            if (existingWishlistItem == null)
            {
                var newWishlistItem = new WishList
                {
                    ApplicationUserId = claim.Value,
                    ProductId = productId,
                    Price = product.Price,
                };

                unitOfWork.WishList.Add(newWishlistItem);
                unitOfWork.Save();

                var successMessage = "Product added to Wishlist successfully";
                return Json(new { success = true, message = successMessage });
            }

            var errorMessage = "Product is already in Wishlist";
            return Json(new { success = false, message = errorMessage });
        }




        public ActionResult RemoveFromWishlist(int id)
        {
            var WishlistItem = unitOfWork.WishList.Get(u => u.WishListId == id);

            if (WishlistItem != null)
            {
                unitOfWork.WishList.Remove(WishlistItem);
                unitOfWork.Save();

                TempData["success"] = "Product removed from Wishlist successfully.";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                TempData["error"] = "Wishlist item not found.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
