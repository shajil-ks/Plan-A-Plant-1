using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plan_A_Plant.Models
{
    public class OrderHeader
    {
        public int Id { get; set; } 

        public string ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        [ValidateNever]
        public ApplicationUser ApplicationUser { get; set; }

        public DateTime OrderDate { get; set; }
        
        public DateTime ShippingDate { get; set; }

        public double OrderTotal { get; set; }

        public string PaymentMethod { get; set; }

        public string? CouponCode { get; set; }
        public int? CouponId { get; set; }


        public string? OrderStatus { get; set; } 

         
        public string ? PaymentStatus { get; set; }   

        public string? TrackingNumber { get; set; }  

        public string? Carrier {  get; set; }    

        public DateTime PaymentDate { get; set; }   

        public DateOnly PaymentDueDate { get; set; }    


        public string? SessionId {  get; set; }  
        public string? PaymentIntentId { get; set; }

        [Required]
        public string MobileNumber { get; set; }
        [Required]
        public string StreetAddress { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string State {  get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Name { get; set; }
       
        public string? CancellationReason { get; set; }
        public DateTime? CancellationRequestDate { get; set; }
        public string? CancellationStatus { get; set; }


    }
}
