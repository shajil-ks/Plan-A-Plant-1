using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Plan_A_Plant.Models
{
    public class MultipleAddress
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z\s]*$", ErrorMessage = "Name must contain only letters and spaces.")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters long.")]
        [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mobile number is required.")]
        [DisplayName("Phone Number")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be exactly 10 digits and contain only numbers.")]
        public string MobileNumber { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Invalid postal code (must be 6 digits starting with 0-9).")]
        public string PostalCode { get; set; }

        public string? ApplicationUserId { get; set; }
        public int? Options { get; set; }


    }
}
