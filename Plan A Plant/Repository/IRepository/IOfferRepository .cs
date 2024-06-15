using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Models;
using Stripe;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IOfferRepository:IRepository <Offer>
    {
        void Update(Offer obj);


      
    }
}
