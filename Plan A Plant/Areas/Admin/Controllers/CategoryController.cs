using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;

namespace Plan_A_Plant.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList;

            try
            {
                // Fetch data from database
                objCategoryList = _unitOfWork.Category.GetAll().ToList();
            }
            catch (Exception ex)
            { 
                TempData["ErrorMessage"] = "There was a problem retrieving categories. Please try again later.";
                objCategoryList = new List<Category>(); 
            }

            return View(objCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            try
            {
                obj.IsActive = true;
                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Add(obj);
                    _unitOfWork.Save();
                    TempData["success"] = "Category Updated Successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while updating the category. Please try again.";
               
            }

            return View(obj);
        }


        public IActionResult Edit(int? Id)
        {
            try
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }

                Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == Id);

                if (categoryFromDb == null)
                {
                    return NotFound();
                }

                return View(categoryFromDb);
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while fetching the category details. Please try again.";
                return RedirectToAction("Index"); 
            }
        }


        [HttpPost]
        public IActionResult Edit(Category obj, int id, [Bind("Id,Name,DisplayOrder,Description,IsActive")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _unitOfWork.Category.Update(obj);
                    _unitOfWork.Save();
                    TempData["success"] = "Category Edited Successfully";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while editing the category. Please try again.";
                
            }

            return View(obj);
        }



        public IActionResult Delete(int? Id)
        {
            try
            {
                if (Id == null || Id == 0)
                {
                    return NotFound();
                }

                Category? categoryFromDb = _unitOfWork.Category.Get(u => u.Id == Id);

                if (categoryFromDb == null)
                {
                    return NotFound();
                }

                return View(categoryFromDb);
            }
            catch (Exception ex)
            {

                TempData["error"] = "An error occurred while fetching the category details for deletion. Please try again.";
                
                return RedirectToAction("Index");
            }
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? Id)
        {
            try
            {
                Category? obj = _unitOfWork.Category.Get(u => u.Id == Id);
                if (obj == null)
                {
                    return NotFound();
                }

                _unitOfWork.Category.Remove(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category Deleted Successfully";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the category. Please try again.";
               

                return RedirectToAction("Index"); 
            }
        }




    }





}
