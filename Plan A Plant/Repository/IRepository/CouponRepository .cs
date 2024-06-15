using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Data;
using Plan_A_Plant.Models;

using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class CouponRepository : Repository<Coupon>, ICouponRepository
    {
        private ApplicationDbContext _db;
        public CouponRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(Coupon obj)
        {
            _db.Coupons.Update(obj);
      
        }


        public Coupon GetCouponByCode(string couponCode)
        {
            return _db.Coupons.FirstOrDefault(c => c.Code == couponCode);
        }
    }
}
