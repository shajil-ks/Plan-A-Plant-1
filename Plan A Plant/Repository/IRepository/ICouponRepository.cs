using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Models;
using Stripe;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface ICouponRepository:IRepository <Models.Coupon>
    {
        void Update(Models.Coupon obj);


      
    }
}
