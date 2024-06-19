using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Plan_A_Plant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OfferController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public OfferController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            try
            {
                List<Offer> objOfferList = _unitOfWork.Offer.GetAll(includeProperties: "Product,Category").ToList();
                return View(objOfferList);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while processing your request. Please try again.";
                return View(new List<Offer>()); 
            }
        }


        public IActionResult Create()
        {
            try
            {
                OfferVM offerVM = new OfferVM
                {
                    Offer = new Offer(),
                    Categories = _unitOfWork.Category.GetAll().ToList(),
                    Products = _unitOfWork.Product.GetAll().ToList()
                };
                return View(offerVM);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while initializing the offer creation form. Please try again.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Create(OfferVM offerVM)
        {
            if (ModelState.IsValid)
            {
                if (offerVM.Offer.Offertype == Offer.OfferType.Category)
                {
                    offerVM.Offer.CategoryId = offerVM.SelectedCategoryId;
                    offerVM.Offer.ProductId = null; // Ensure ProductId is null

                    // Check if an offer already exists for the selected category
                    var existingCategoryOffer = _unitOfWork.Offer.GetAll()
                        .FirstOrDefault(o => o.CategoryId == offerVM.Offer.CategoryId);

                    if (existingCategoryOffer != null)
                    {
                        TempData["error"] = "An offer already exists for the selected Category";
                        offerVM.Categories = _unitOfWork.Category.GetAll().ToList();
                        offerVM.Products = _unitOfWork.Product.GetAll().ToList();
                        return View(offerVM);
                    }
                }
                else if (offerVM.Offer.Offertype == Offer.OfferType.Product)
                {
                    offerVM.Offer.ProductId = offerVM.SelectedProductId;
                    offerVM.Offer.CategoryId = null; // Ensure CategoryId is null

                    // Check if an offer already exists for the selected product
                    var existingProductOffer = _unitOfWork.Offer.GetAll()
                        .FirstOrDefault(o => o.ProductId == offerVM.Offer.ProductId);

                    if (existingProductOffer != null)
                    {
                        TempData["error"] = "An offer already exists for the selected Product";
                        offerVM.Categories = _unitOfWork.Category.GetAll().ToList();
                        offerVM.Products = _unitOfWork.Product.GetAll().ToList();
                        return View(offerVM);
                    }
                }

                // Add offer to the repository
                _unitOfWork.Offer.Add(offerVM.Offer);
                _unitOfWork.Save();

                // Update discounted prices
                UpdateDiscountedPrices();
                TempData["success"] = "Offer created succesfully";
                return RedirectToAction(nameof(Index));
            }

            offerVM.Categories = _unitOfWork.Category.GetAll().ToList();
            offerVM.Products = _unitOfWork.Product.GetAll().ToList();
            return View(offerVM);
        }

        public IActionResult Edit(int id)
        {
            try
            {
                var offer = _unitOfWork.Offer.Get(o => o.OfferId == id, includeProperties: "Product,Category");
                if (offer == null)
                {
                    return NotFound();
                }

                OfferVM offerVM = new OfferVM
                {
                    Offer = offer,
                    Categories = _unitOfWork.Category.GetAll().ToList(),
                    Products = _unitOfWork.Product.GetAll().ToList(),
                    SelectedCategoryId = offer.CategoryId,
                    SelectedProductId = offer.ProductId
                };
                return View(offerVM);
            }
            catch (Exception ex)
            {                
                TempData["error"] = "An error occurred while fetching or initializing the offer for editing. Please try again.";
                return RedirectToAction("Index"); 
            }
        }


        [HttpPost]
        public IActionResult Edit(OfferVM offerVM)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (offerVM.Offer.Offertype == Offer.OfferType.Category)
                    {
                        offerVM.Offer.CategoryId = offerVM.SelectedCategoryId;
                        offerVM.Offer.ProductId = null; // Ensure ProductId is null
                    }
                    else if (offerVM.Offer.Offertype == Offer.OfferType.Product)
                    {
                        offerVM.Offer.ProductId = offerVM.SelectedProductId;
                        offerVM.Offer.CategoryId = null; // Ensure CategoryId is null
                    }

                    _unitOfWork.Offer.Update(offerVM.Offer);
                    _unitOfWork.Save();

                    // Update discounted prices
                    UpdateDiscountedPrices();
                    TempData["success"] = "Offer Updated successfully";
                    return RedirectToAction(nameof(Index));
                }

                offerVM.Categories = _unitOfWork.Category.GetAll().ToList();
                offerVM.Products = _unitOfWork.Product.GetAll().ToList();
                return View(offerVM);
            }
            catch (Exception ex)
            {
               TempData["error"] = "An error occurred while updating the offer. Please try again.";
                // Reload Categories and Products to ensure the view can be rendered correctly
                offerVM.Categories = _unitOfWork.Category.GetAll().ToList();
                offerVM.Products = _unitOfWork.Product.GetAll().ToList();
                return View(offerVM);
            }
        }


        public IActionResult Delete(int id)
        {
            try
            {
                var offer = _unitOfWork.Offer.Get(o => o.OfferId == id, includeProperties: "Product,Category");
                if (offer == null)
                {
                    return NotFound();
                }

                return View(offer);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while retrieving the offer for deletion. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            try
            {
                var offer = _unitOfWork.Offer.Get(o => o.OfferId == id);

                if (offer == null)
                {
                    return NotFound();
                }

                _unitOfWork.Offer.Remove(offer);
                _unitOfWork.Save();

                // Update discounted prices
                UpdateDiscountedPrices();
                TempData["success"] = "Offer Deleted successfully";
            }
            catch (Exception ex)
            {              
                TempData["error"] = "An error occurred while deleting the offer. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }


        private void UpdateDiscountedPrices()
        {
            var products = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

            foreach (var product in products)
            {
                var categoryOffer = _unitOfWork.Offer.GetAll()
                    .Where(o => o.CategoryId == product.CategoryId && o.Offertype == Offer.OfferType.Category)
                    .OrderByDescending(o => o.OfferDiscount)
                    .FirstOrDefault();

                var productOffer = _unitOfWork.Offer.GetAll()
                    .Where(o => o.ProductId == product.Id && o.Offertype == Offer.OfferType.Product)
                    .OrderByDescending(o => o.OfferDiscount)
                    .FirstOrDefault();

                double maxDiscount = 0;

                if (categoryOffer != null)
                {
                    maxDiscount = categoryOffer.OfferDiscount;
                }

                if (productOffer != null && productOffer.OfferDiscount > maxDiscount)
                {
                    maxDiscount = productOffer.OfferDiscount;
                }

                if (maxDiscount > 0)
                {
                    product.DiscountedPrice = Math.Round(product.Price * (1 - maxDiscount / 100), 2);
                }
                else
                {
                    product.DiscountedPrice = product.Price;
                }

                _unitOfWork.Product.Update(product);
            }

            _unitOfWork.Save();
        }

    }
}
