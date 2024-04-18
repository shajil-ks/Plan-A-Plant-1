using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IProductRepository:IRepository <Product>
    {
        void Update(Product obj);
       
    }
}
