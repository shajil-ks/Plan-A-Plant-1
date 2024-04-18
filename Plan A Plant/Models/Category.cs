using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;

namespace Plan_A_Plant.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]  
        public string Name { get; set; }
        [Range(0,10)]
        public int DisplayOrder {  get; set; }
        [MaxLength(30)]    
        public string Description { get; set; } 

        public bool IsActive { get; set; }



    }
}
