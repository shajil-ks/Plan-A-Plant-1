namespace Plan_A_Plant.Models.ViewModels
{
    public class DashboardVM
    {
       public IEnumerable<Product> product { get; set; }
        public IEnumerable<Category> categors { get; set; }
        public IEnumerable<OrderHeader> orderHeader { get; set; }
        public IEnumerable<OrderDetail> orderDetail { get; set; }

        public List<Product> TopSellingProducts { get; set; }
        public Dictionary<int, int> ProductQuantitiesSold { get; set; }

        public List<Category> TopSellingCategories { get; set; }
        public Dictionary<int, int> CategorySales    { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int ShippedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int CancelledCount { get; set; }
        public int BillingCount { get; set; }
        public double TotalRevenueLastWeek { get; set; }
        public double NumberOfOrdersLastWeek { get; set; }
        public double TotalRevenueToday { get; set; }
        public double TotalRevenueThisWeek { get; set; }
        public double TotalRevenueThisMonth { get; set; }
        public double TotalRevenueThisYear { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<double> ChartData { get; set; }


    }
}
