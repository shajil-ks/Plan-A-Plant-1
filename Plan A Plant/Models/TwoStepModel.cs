using System.ComponentModel.DataAnnotations;

namespace Plan_A_Plant.Models
{
    public class TwoStepModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }
        public bool RememberMe { get; set; }


    }
}
