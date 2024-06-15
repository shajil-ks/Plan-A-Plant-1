using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IOrderHeaderRepository:IRepository <OrderHeader>
    {
        void Update(OrderHeader obj);

        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);

        void UpdateStripePaymentID(int id,string sessionId, string paymentIntentId);
    }
}
