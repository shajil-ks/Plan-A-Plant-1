using Plan_A_Plant.Data;
using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public class MultipleAddressRepository : Repository<MultipleAddress>, IMultipleAddressRepository
    {
        private ApplicationDbContext _db;
        public MultipleAddressRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(MultipleAddress obj)
        {
            _db.MultipleAddresses.Update(obj);
        }
    }
}
