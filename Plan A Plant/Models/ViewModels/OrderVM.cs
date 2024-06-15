namespace Plan_A_Plant.Models.ViewModels
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; }
        public IEnumerable<OrderDetail> OrderDetail { get; set; }

        public CancellationRequestVM CancellationRequest { get; set; }
    }
}
