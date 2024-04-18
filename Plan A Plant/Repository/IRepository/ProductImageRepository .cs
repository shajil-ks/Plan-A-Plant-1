using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {
        private ApplicationDbContext _db;
        public ProductImageRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(ProductImage obj)
        {
            _db.ProductImages.Update(obj);
      
        }
    }
}
