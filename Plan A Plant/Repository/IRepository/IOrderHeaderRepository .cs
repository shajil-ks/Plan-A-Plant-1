using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IOrderHeaderRepository:IRepository <OrderHeader>
    {
        void Update(OrderHeader obj);
        
    }
}
