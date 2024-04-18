using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(OrderHeader obj)
        {
            _db.OrderHeaders.Update(obj);
      
        }
    }
}
