using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;
using System.Security.Claims;


namespace Plan_A_Plant.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Roles = SD.Role_User)]
    public class WalletController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        

        public WalletController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
           
        }

        // GET: Wallet/History
        public IActionResult Index()
        {
            var clamisIdentity = (ClaimsIdentity)User.Identity;
            var userId = clamisIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<WalletTransaction> objWallletTransactionList;
            objWallletTransactionList= _unitOfWork.WalletTransaction.GetAll(u => u.UserId == userId).ToList();

            return View(objWallletTransactionList);
        }



    }
}
