using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan_A_Plant.Models
{
    public class ShoppingCart
    {
        [Key]
        public int ShopCartId { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        
        public Product Product { get; set; }

        [Range(1, 50, ErrorMessage = "Count must be between 1 and 50.")]
        public int Count { get; set; }

        public string ApplicationUserId {  get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever] 
        public ApplicationUser ApplicationUser { get; set; }
        [NotMapped]
        public double Price { get; set; }   


    }
}
