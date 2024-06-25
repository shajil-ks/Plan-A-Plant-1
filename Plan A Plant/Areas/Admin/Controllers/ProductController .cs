using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Plan_A_Plant.Utility;
using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            try
            {
                // Fetch data from db
                List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
                return View(objProductList);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the product list. Please try again.";
                return View(new List<Product>());
            }
        }


        public IActionResult Upsert(int? Id)
        {
            try
            {
                ProductVM productVM = new ProductVM
                {
                    CategoryList = _unitOfWork.Category.GetAll()
                        .Select(u => new SelectListItem
                        {
                            Text = u.Name,
                            Value = u.Id.ToString()
                        }),
                    Product = new Product()
                };

                if (Id == null || Id == 0)
                {
                    // Create
                    return View(productVM);
                }
                else
                {
                    // Update
                    productVM.Product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "ProductImages");
                    if (productVM.Product == null)
                    {
                        return NotFound();
                    }
                    return View(productVM);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while processing your request. Please try again.";
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (productVM.Product.Id == 0)
                    {
                        // Set the DiscountedPrice to the Price when creating a new product
                        productVM.Product.DiscountedPrice = productVM.Product.Price;
                        _unitOfWork.Product.Add(productVM.Product);
                    }
                    else
                    {
                        _unitOfWork.Product.Update(productVM.Product);
                    }

                    _unitOfWork.Save();

                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    if (files != null)
                    {
                        foreach (IFormFile file in files)
                        {
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            string produtPath = @"images\products\product-" + productVM.Product.Id;
                            string finalPath = Path.Combine(wwwRootPath, produtPath);

                            if (!Directory.Exists(finalPath))
                                Directory.CreateDirectory(finalPath);
                            using (var fileStrem = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                            {
                                file.CopyTo(fileStrem);
                            }

                            ProductImage productImage = new()
                            {
                                ImageUrl = @"\" + produtPath + @"\" + fileName,
                                ProductId = productVM.Product.Id,
                            };
                            if (productVM.Product.ProductImages == null)
                                productVM.Product.ProductImages = new List<ProductImage>();
                            productVM.Product.ProductImages.Add(productImage);
                        }

                        _unitOfWork.Product.Update(productVM.Product);
                        _unitOfWork.Save();
                    }

                    TempData["success"] = "Product Created/Updated Successfully";
                    return RedirectToAction("Index");
                }
                else
                {
                    productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.Name,
                        Value = u.Id.ToString()
                    });

                    return View(productVM);
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while processing your request. Please try again.";

                productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });

                return View(productVM);
            }
        }





        public IActionResult DeleteImage(int imageId)
        {
            try
            {
                var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
                if (imageToBeDeleted == null)
                {
                    TempData["error"] = "Image not found.";
                    return RedirectToAction(nameof(Index));
                }

                int productId = imageToBeDeleted.ProductId;
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImage.Remove(imageToBeDeleted);
                _unitOfWork.Save();
                TempData["success"] = "Image deleted successfully";

                return RedirectToAction(nameof(Upsert), new { id = productId });
            }
            catch (Exception ex)
            {               
                TempData["error"] = "An error occurred while processing your request. Please try again.";
                
                return RedirectToAction(nameof(Index));
            }
        }



        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
            
            return Json(new { data = objProductList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if (productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }



            string produtPath = @"images\products\product-" + id;
            string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, produtPath);

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            if (Directory.Exists(finalPath))
            {
                string[] fiilePaths = Directory.GetFiles(finalPath);
                foreach (string fiilePath in fiilePaths)
                {
                    System.IO.File.Delete(fiilePath);
                }
                Directory.Delete(finalPath);
            }


            return Json(new { success = true, message = "Delete Successful" });
        }


        #endregion



    }
}
