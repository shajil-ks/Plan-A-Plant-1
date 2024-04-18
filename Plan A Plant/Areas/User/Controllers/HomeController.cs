using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository;
using Plan_A_Plant.Repository.IRepository;
using System.Diagnostics;
using System.Security.Claims;

namespace Plan_A_Plant.Areas.User.Controllers
{
    [Area("User")]
    public class HomeController : Controller
    {
      
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;


        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;   
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            return View(productList);


        }

        
        public IActionResult Search(string searchString, int? categoryId)
        {
            // Get all products
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll(includeProperties: "Category,ProductImages");

            // Filter by category if provided
            if (categoryId != null)
            {
                productList = productList.Where(p => p.CategoryId == categoryId);
            }

            // Filter by search string if provided
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.Trim().ToLower(); // Convert search string to lowercase for case-insensitive comparison

                // Filter by product name or category name containing the search string
                productList = productList.Where(p =>
                    p.Name.ToLower().Contains(searchString) ||
                    p.Category.Name.ToLower().Contains(searchString));
            }

            var categories = _unitOfWork.Category.GetAll().ToList();
            ViewBag.Categories = categories; // Pass categories to the layout

            return View("Index", productList.ToList());
        }



        public IActionResult ViewProduct(int Id )
        {
            ShoppingCart cart = new()
            {
                  Product = _unitOfWork.Product.Get(u => u.Id == Id, includeProperties: "Category,ProductImages"),
                  Count=1,
                  ProductId=Id
            };
            
            return View(cart);
        }


        [HttpPost]
        [Authorize]
        public IActionResult ViewProduct(ShoppingCart shoppingCart)
        {
            int Limit = 5;
            var clamisIdentity=(ClaimsIdentity)User.Identity;
            var userId= clamisIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;  
            shoppingCart.ApplicationUserId = userId; 
            
            ShoppingCart cartFromDb=_unitOfWork.ShoppingCart.Get(u=>u.ApplicationUserId == userId && 
            u.ProductId == shoppingCart.ProductId);
            Product product = _unitOfWork.Product.Get(p => p.Id == shoppingCart.ProductId);

            if (shoppingCart.Count > product.Qty)
            {
                var quantiy = product.Qty;
                TempData["StockErorr"] = "We have not enough stock !!!! stock is only up to " + quantiy;
                return RedirectToAction(nameof(ViewProduct));

            }
            else if(shoppingCart.Count > Limit)
            {
                TempData["LimitError"] = "You Exceeded the Maximum Limit !!!";
                return RedirectToAction(nameof(ViewProduct));
            }

            

                if (cartFromDb != null) 
            {  //shopping carexisist
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCart. Update(cartFromDb);
                

            }
            else
            {  //create newrecord
                _unitOfWork.ShoppingCart.Add(shoppingCart);
                
            }
            
            _unitOfWork.Save();
            TempData["Success"] = "Cart Updated successfully";
            return RedirectToAction (nameof(Index));
            
        }





        public IActionResult Test()  
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
