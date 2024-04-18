using Plan_A_Plant.Models;

namespace Plan_A_Plant.Repository.IRepository
{
    public interface ICategoryRepository:IRepository <Category>
    {
        void Update(Category obj);
        
    }
}
