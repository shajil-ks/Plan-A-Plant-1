using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
      
        }
    }
}
