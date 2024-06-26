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

        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index(int page = 1, int itemsPerPage = 5)
        {
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            IEnumerable<Product> productList = _unitOfWork.Product.GetAll();
            IEnumerable<Category> catogoryList = _unitOfWork.Category.GetAll();

            //For Product
            var ProductQuantitiesSold = productList.ToDictionary(
                 p => p.Id,
                 p => _unitOfWork.OrderDetail.GetAll().Where(od => od.ProductId == p.Id).Sum(od => od.Count));


            var topSellingProducts = productList
                .OrderByDescending(p => ProductQuantitiesSold.GetValueOrDefault(p.Id, 0))
                .Take(5)
                .ToList();

            var categorySales = catogoryList.ToDictionary(
                    c => c.Id,
                    c => _unitOfWork.OrderDetail.GetAll()
                                     .Where(od => od.Product.CategoryId == c.Id)
                                     .Sum(od => od.Count)
                        );
            var topSellingCategories = catogoryList
                .OrderByDescending(c => categorySales.GetValueOrDefault(c.Id, 0))
                .Take(5)
                .ToList();

            int shippedCount = orderHeaders.Count(u => u.OrderStatus == "Shipped");
            int approvedCount = orderHeaders.Count(u => u.OrderStatus == "Approved");
            int cancelledCount = orderHeaders.Count(u => u.OrderStatus == "Cancelled");
            int productCount = productList.Count();
            int orderCount = orderHeaders.Count();
            int categoryCount = catogoryList.Count();
            decimal totalSales = 0;

            DateTime today = DateTime.Now;

            DateTime oneWeekAgo = today.AddDays(-7);
            IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= oneWeekAgo && order.OrderDate <= today).OrderByDescending(order => order.OrderDate);
            int numberOfOrdersLastWeek = ordersLastWeek.Count();
            double totalRevenueLastWeek = (double)ordersLastWeek.Sum(order => order.OrderTotal);

            DateTime oneMonthAgo = today.AddMonths(-1);
            DateTime oneYearAgo = today.AddYears(-1);

            double totalRevenueToday = (double)orderHeaders
        .Where(order => order.OrderDate.Date == today.Date)
        .Sum(order => order.OrderTotal);

            double totalRevenueThisWeek = (double)orderHeaders
                .Where(order => order.OrderDate >= oneWeekAgo && order.OrderDate <= today)
                .Sum(order => order.OrderTotal);

            double totalRevenueThisMonth = (double)orderHeaders
                .Where(order => order.OrderDate >= oneMonthAgo && order.OrderDate <= today)
                .Sum(order => order.OrderTotal);

            double totalRevenueThisYear = (double)orderHeaders
                .Where(order => order.OrderDate >= oneYearAgo && order.OrderDate <= today)
                .Sum(order => order.OrderTotal);

            // Filter orders for the last three weeks
            var chartData = new List<double> { totalRevenueToday, totalRevenueThisWeek, totalRevenueThisMonth, totalRevenueThisYear };
            var chartLabels = new List<string> { "Today", "This Week", "This Month", "This Year" };




            foreach (var order in orderHeaders)
            {
               totalSales +=(decimal) order.OrderTotal;
            }


            var viewModel = new DashboardVM
            {

                categors = catogoryList,
                product = productList,
                OrderCount = orderCount,
                ProductCount = productCount,
                CategoryCount = categoryCount,
                orderHeader = ordersLastWeek,
                TotalSales = (decimal)totalSales,
                ApprovedCount = approvedCount,
                CancelledCount = cancelledCount,
                ShippedCount = shippedCount,
                TotalRevenueToday = totalRevenueToday,
                TotalRevenueThisWeek = totalRevenueThisWeek,
                TotalRevenueThisMonth = totalRevenueThisMonth,
                TotalRevenueThisYear = totalRevenueThisYear,
                NumberOfOrdersLastWeek = numberOfOrdersLastWeek,
                CurrentPage = page,
                ItemsPerPage = itemsPerPage,
                TopSellingProducts = topSellingProducts,
                ProductQuantitiesSold = ProductQuantitiesSold,
                TopSellingCategories = topSellingCategories,
                CategorySales = categorySales,

            };
            viewModel.orderHeader = ordersLastWeek.Skip((page - 1) * itemsPerPage).Take(itemsPerPage);
            return View(viewModel);
        }



        //public IActionResult Invoice()
        //{
        //    IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
        //    DateTime today = DateTime.Now;
        //    DateTime lastWeek = today.AddDays(-7);

        //    IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= lastWeek && order.OrderDate <= today).OrderByDescending(order => order.OrderDate);
        //    double totalRevenueLastWeek = (double)ordersLastWeek.Sum(order => order.OrderTotal);
        //    int cancelledCount = ordersLastWeek.Count(u => u.OrderStatus == "Cancelled");
        //    int orderCount = ordersLastWeek.Count();
        //    var viewModel = new DashboardVM
        //    {
        //        orderHeader = ordersLastWeek,
        //        TotalRevenueLastWeek = totalRevenueLastWeek,
        //        CancelledCount = cancelledCount,
        //        OrderCount = orderCount

        //    };
        //    return View(viewModel);
        //}

        public IActionResult Invoice()
        {
            // Retrieve all OrderHeader records including related ApplicationUser data
            IEnumerable<OrderHeader> orderHeaders = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");

            // Calculate total revenue from all orders
            double totalRevenue = (double)orderHeaders.Sum(order => order.OrderTotal);

            // Count the number of cancelled orders
            int cancelledCount = orderHeaders.Count(order => order.OrderStatus == "Cancelled");

            // Count the total number of orders
            int orderCount = orderHeaders.Count();

            // Create and populate the DashboardVM with all orders and calculated statistics
            var viewModel = new DashboardVM
            {
                orderHeader = orderHeaders,      
                TotalRevenueLastWeek = totalRevenue, 
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
                DateTime oneWeekAgo = today.AddDays(-7);
                IEnumerable<OrderHeader> ordersLastWeek = orderHeaders.Where(order => order.OrderDate >= oneWeekAgo && order.OrderDate <= today).OrderByDescending(order => order.OrderDate);
                DateTime oneDayAgo = today.AddDays(-1);
                DateTime oneMonthAgo = today.AddMonths(-1);
                DateTime oneYearAgo = today.AddYears(-1);

                // Filter orders for different periods
                IEnumerable<OrderHeader> ordersToday = orderHeaders.Where(order => order.OrderDate.Date == today.Date);
                IEnumerable<OrderHeader> ordersThisWeek = orderHeaders.Where(order => order.OrderDate >= oneWeekAgo && order.OrderDate <= today);
                IEnumerable<OrderHeader> ordersThisMonth = orderHeaders.Where(order => order.OrderDate >= oneMonthAgo && order.OrderDate <= today);
                IEnumerable<OrderHeader> ordersThisYear = orderHeaders.Where(order => order.OrderDate >= oneYearAgo && order.OrderDate <= today);

                // Calculate total revenue for different periods
                double totalRevenueToday = (double)ordersToday.Sum(order => order.OrderTotal);
                double totalRevenueThisWeek = (double)ordersThisWeek.Sum(order => order.OrderTotal);
                double totalRevenueThisMonth = (double)ordersThisMonth.Sum(order => order.OrderTotal);
                double totalRevenueThisYear = (double)ordersThisYear.Sum(order => order.OrderTotal);

                // Prepare chart data and labels
                var chartData = new List<double> { totalRevenueToday, totalRevenueThisWeek, totalRevenueThisMonth, totalRevenueThisYear };
                var chartLabels = new List<string> { "Today", "This Week", "This Month", "This Year" };

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


  

