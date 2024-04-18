namespace Plan_A_Plant.Models.ViewModels
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList {  get; set; } 
        public OrderHeader OrderHeader { get; set; }    
       

    }
}
