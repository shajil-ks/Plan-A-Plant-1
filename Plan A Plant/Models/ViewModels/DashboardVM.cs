namespace Plan_A_Plant.Models.ViewModels
{
    public class DashboardVM
    {
        public IEnumerable<Product> product { get; set; }
        public IEnumerable<Category> categors { get; set; }
        public IEnumerable<OrderHeader> orderHeader { get; set; }
        public IEnumerable<OrderDetail> orderDetail { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSales { get; set; }
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int ShippedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int CancelledCount { get; set; }
        public int BillingCount { get; set; }
        public double TotalRevenueLastWeek { get; set; }
        public int NumberOfOrdersLastWeek { get; set; }
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public List<string> ChartLabels { get; set; }
        public List<double> ChartData { get; set; }


    }
}
