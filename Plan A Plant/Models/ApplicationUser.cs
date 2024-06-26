using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Name must contain only letters and spaces.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; }
        [Required]  
        public string? StreetAddress { get; set; }
        [Required]  
        public string? City { get; set; }
        [Required]  
        public string? State { get; set; }
        [Required]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Invalid postal code (must be 6 digits starting with 0-9).")]
        public string? PostalCode { get; set; }  

        public int? Wallet {  get; set; }    

        public string WalletSessionId { get; set; } 

        public string WalletPaymentIntentId { get; set; }

        public string UsedCoupons { get; set; }

        
    }
}
