using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan_A_Plant.Models
{
    public class Offer
    {
        [Key]
        public int OfferId { get; set; }

        [Required]
        public string OfferName { get; set; }
        [Required]
        public OfferType Offertype { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Discount must be between 0 and 100.")]
        public double OfferDiscount { get; set; }

        [Required]
        public string OfferDescription { get; set; }

        public enum OfferType
        {
            Category,
            Product,
            Referral
        }


        public int? ProductId { get; set; }
        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public int? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category Category { get; set; }

    }



}
