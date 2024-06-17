using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IMultipleAddressRepository : IRepository<MultipleAddress>
    {
        void Update(MultipleAddress obj);
    }
}
