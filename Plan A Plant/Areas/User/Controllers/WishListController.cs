using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository;
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
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    
                    return Unauthorized();
                }

                var WishlistItems = unitOfWork.WishList.GetAll(
                    u => u.ApplicationUserId == userId,
                    includeProperties: "Product.ProductImages"
                );

                return View(WishlistItems);
            }
            catch (Exception ex)
            {
                
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }


        [HttpPost]
        public JsonResult AddToWishlist(int productId)
        {
            try
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

                if (claim == null || string.IsNullOrEmpty(claim.Value))
                {
                    return Json(new { success = false, message = "User identity not found" });
                }

                var existingWishlistItem = unitOfWork.WishList.Get(u => u.ApplicationUserId == claim.Value && u.ProductId == productId);
                Product product = unitOfWork.Product.Get(p => p.Id == productId);

                if (product == null)
                {
                    return Json(new { success = false, message = "Product not found" });
                }

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
            catch (Exception ex)
            {
                
                return Json(new { success = false, message = "An error occurred while processing your request" });
            }
        }





        public ActionResult RemoveFromWishlist(int id)
        {
            try
            {
                var wishlistItem = unitOfWork.WishList.Get(u => u.WishListId == id);

                if (wishlistItem != null)
                {
                    unitOfWork.WishList.Remove(wishlistItem);
                    unitOfWork.Save();

                    TempData["success"] = "Product removed from Wishlist successfully.";
                }
                else
                {
                    TempData["error"] = "Wishlist item not found.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
               
                TempData["error"] = "An error occurred while processing your request.";
                return RedirectToAction(nameof(Index));
            }
        }

    }
}
