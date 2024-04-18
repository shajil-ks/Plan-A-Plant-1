using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IOrderDetailRepository:IRepository <OrderDetail>
    {
        void Update(OrderDetail obj);
        
    }
}
