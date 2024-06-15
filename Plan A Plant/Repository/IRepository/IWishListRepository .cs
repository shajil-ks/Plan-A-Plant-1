using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IWishListRepository:IRepository <WishList>
    {
        void Update(WishList obj);
        
    }
}
