using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Models.ViewModels
{
    public class CancellationRequestVM
    {

        public int OrderId { get; set; }
        [Required]
        public string Reason { get; set; }

        public IEnumerable<OrderDetail> OrderDetail { get; set; }
    }
}
