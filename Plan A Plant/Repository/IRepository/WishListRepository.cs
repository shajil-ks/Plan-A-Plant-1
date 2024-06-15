using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class WishListRepository : Repository<WishList>, IWishListRepository
    {
        private ApplicationDbContext _db;
        public WishListRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(WishList obj)
        {
            _db.WishLists.Update(obj);
      
        }
    }
}
