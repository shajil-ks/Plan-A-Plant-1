using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IShoppingCartRepository:IRepository <ShoppingCart>
    {
        void Update(ShoppingCart obj);
        
    }
}
