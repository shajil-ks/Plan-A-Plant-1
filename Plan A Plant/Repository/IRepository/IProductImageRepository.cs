using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface IProductImageRepository:IRepository <ProductImage>
    {
        void Update(ProductImage obj);
        
    }
}
