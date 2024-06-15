using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Models
{
    public class Coupon
    {
        [Key]
        public int CouponId { get; set; }

        public string? ApplicationUserId { get; set; }  

        [Required]
        public string Code { get; set; }

        [Required]
        public string Description { get; set; } 

        public DiscountType Type { get; set; }
        [Range(0, double.MaxValue, ErrorMessage = "Discount Amount should be a positive value.")]
        public double? DiscountAmount { get; set; }
        [Range(1, 100, ErrorMessage = "Discount Percentage should be between 1-100")]
        public double? DiscountPercentage { get; set; }
        public string? IsValid { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Minimum Amount should be a positive value.")]
        public double MinimumAmount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Maximum Amount should be a positive value.")]
        public double MaximumAmount { get; set; }
        [Required]
        public DateTime ExpiryDate { get; set; }    

       

        public enum DiscountType
        {
            DiscountPercentage,
            DiscountAmount
        }
    }
}

