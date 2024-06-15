using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Data;
using Plan_A_Plant.Models;

using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class OfferRepository : Repository<Offer>, IOfferRepository
    {
        private ApplicationDbContext _db;
        public OfferRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(Offer obj)
        {
            _db.Offers.Update(obj);
      
        }


        
    }
}
