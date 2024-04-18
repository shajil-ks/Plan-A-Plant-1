﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required]
        public string Name { get; set; }
        [Required]  
        public string? StreetAddress { get; set; }
        [Required]  
        public string? City { get; set; }
        [Required]  
        public string? State { get; set; }
        [Required]  
        public string? PostalCode { get; set; }  


    }
}