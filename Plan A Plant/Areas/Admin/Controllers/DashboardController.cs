using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Plan_A_Plant.Models.ViewModels;
using Plan_A_Plant.Models;
using Plan_A_Plant.Repository.IRepository;
using Plan_A_Plant.Utility;

namespace Plan_A_Plant.Areas.Admin.Controllers
{


    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashBoardController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public DashboardVM dashboardVm { get; set; }
        public DashBoardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public IActionResult Index(int page = 1, int itemsPerPage = 5)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll();
            IEnumerable<Category> catogoryList = _unitOfWork.Category.GetAll();
            int shippedCount = orderHeaders.Count(u => u.OrderStatus == "Shipped");
            int approvedCount = orderHeaders.Count(u => u.OrderStatus == "Approved");
            int cancelledCount = orderHeaders.Count(u => u.OrderStatus == "Cancelled");
            int productCount = productList.Count();
            int orderCount = orderHeaders.Count();
            int categoryCount = catogoryList.Count();
            double totalSales = orderHeaders.Sum(order => order.OrderTotal);
            DateTime today = DateTime.Now;
            DateTime lastWeek = today.AddDays(-7);
            IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today).OrderByDescending(order => order.OrderDate);

            int numberOfOrdersLastWeek = ordersLastWeek.Count();
            double totalRevenueLastWeek = (double)ordersLastWeek.Sum(order => order.OrderTotal);
            DateTime currentDate = DateTime.Now;
            DateTime lastWeek1 = today.AddDays(-7);
            DateTime lastWeek2 = today.AddDays(-14);
            DateTime lastWeek3 = today.AddDays(-21);

            // Filter orders for the last three weeks
            IEnumerable<OrderHeader> ordersWeek1 = orderHeaders
            .Where(order => order.OrderDate >= today && order.OrderDate <= lastWeek1)
            .OrderByDescending(order => order.OrderDate);

            IEnumerable<OrderHeader> ordersWeek2 = orderHeaders
                .Where(order => order.OrderDate >= lastWeek1 && order.OrderDate < lastWeek2)
                .OrderByDescending(order => order.OrderDate);

            IEnumerable<OrderHeader> ordersWeek3 = orderHeaders
                .Where(order => order.OrderDate >= lastWeek1 && order.OrderDate <= lastWeek2)
                .OrderByDescending(order => order.OrderDate);


            double totalRevenueWeek1 = (double)ordersWeek1.Sum(order => order.OrderTotal);
            double totalRevenueWeek2 = (double)ordersWeek2.Sum(order => order.OrderTotal);
            double totalRevenueWeek3 = (double)ordersLastWeek.Sum(order => order.OrderTotal);

            var chartData = new List<double> { totalRevenueWeek1, totalRevenueWeek2, totalRevenueWeek3 };
            var chartLabels = new List<string> { "Week 1", "Week 2", "Week 3" };
            foreach (var order in orderHeaders)
            {
                totalSales += order.OrderTotal;
            }

            var viewModel = new DashboardVM
            {

                categors = catogoryList,
                product = productList,
                orderHeader = ordersLastWeek,
                OrderCount = orderCount,
                ProductCount = productCount,
                CategoryCount = categoryCount,
                TotalSales = (decimal)totalSales,
                ApprovedCount = approvedCount,
                CancelledCount = cancelledCount,
                ShippedCount = shippedCount,
                TotalRevenueLastWeek = totalRevenueLastWeek,
                NumberOfOrdersLastWeek = numberOfOrdersLastWeek,
                CurrentPage = page,
                ItemsPerPage = itemsPerPage
            };
            viewModel.orderHeader = ordersLastWeek.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);
            return View(viewModel);
        }



        public IActionResult Invoice()
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            DateTime today = DateTime.Now;
            DateTime lastWeek = today.AddDays(-7);

            IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today).OrderByDescending(order => order.OrderDate);
            double totalRevenueLastWeek = (double)ordersLastWeek.Sum(order => order.OrderTotal);
            int cancelledCount = ordersLastWeek.Count(u => u.OrderStatus == "Cancelled");
            int orderCount = ordersLastWeek.Count();
            var viewModel = new DashboardVM
            {
                orderHeader = ordersLastWeek,
                TotalRevenueLastWeek = totalRevenueLastWeek,
                CancelledCount = cancelledCount,
                OrderCount = orderCount

            };
            return View(viewModel);
        }
        [HttpPost]
        public JsonResult Index()
        {
            try
            {
                IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

                DateTime today = DateTime.Now;
                DateTime lastWeek = today.AddDays(-7);
                IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today).OrderByDescending(order => order.OrderDate);
                int numberOfOrdersLastWeek = ordersLastWeek.Count();
                double totalRevenueLastWeek = (double)ordersLastWeek.Sum(order => order.OrderTotal);
                DateTime currentDate = DateTime.Now;
                DateTime lastWeek1 = today.AddDays(-7);
                DateTime lastWeek2 = today.AddDays(-14);
                DateTime lastWeek3 = today.AddDays(-21);
                // Filter orders for the last three weeks
                IEnumerable<OrderHeader> ordersWeek1 = orderHeaders
                    .Where(order => order.OrderDate >= lastWeek1 && order.OrderDate <= today)
                    .OrderByDescending(order => order.OrderDate);

                IEnumerable<OrderHeader> ordersWeek2 = orderHeaders
                    .Where(order => order.OrderDate >= lastWeek2 && order.OrderDate < lastWeek1)
                    .OrderByDescending(order => order.OrderDate);

                IEnumerable<OrderHeader> ordersWeek3 = orderHeaders
                    .Where(order => order.OrderDate >= lastWeek1 && order.OrderDate < lastWeek2)
                    .OrderByDescending(order => order.OrderDate);


                double totalRevenueWeek1 = (double)ordersWeek1.Sum(order => order.OrderTotal);
                double totalRevenueWeek2 = (double)ordersWeek2.Sum(order => order.OrderTotal);
                double totalRevenueWeek3 = (double)ordersLastWeek.Sum(order => order.OrderTotal);

                var chartData = new List<double> { totalRevenueWeek1, totalRevenueWeek2, totalRevenueWeek3 };
                var chartLabels = new List<string> { "Week 1", "Week 2", "Week 3" };
                // Replace with your actual logic

                // Return JSON result
                return Json(new { ChartLabels = chartLabels, ChartData = chartData });
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                return Json(new { error = "An error occurred while processing the request." });
            }
        }

    }


}


  

