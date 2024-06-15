using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Models.ViewModels
{
    public class OfferVM
    {
        public Offer Offer { get; set; }

        [ValidateNever]
        public IEnumerable<Category> Categories { get; set; }

        [ValidateNever]
        public IEnumerable<Product> Products { get; set; }

       
        public int? SelectedCategoryId { get; set; }

        
        public int? SelectedProductId { get; set; }

    }
}
