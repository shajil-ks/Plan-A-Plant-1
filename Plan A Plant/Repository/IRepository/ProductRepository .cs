 using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db): base(db) 
        {
            _db = db;
        }

      
        public void Update(Product obj)
        {
            var objFromDb= _db.Products.FirstOrDefault(u=>u.Id==obj.Id);   
            if (objFromDb!=null) 
            {
                objFromDb.ProductImages = obj.ProductImages;
                objFromDb.Name = obj.Name;  
                objFromDb.Description = obj.Description;
                objFromDb.Price = obj.Price;    
                objFromDb.Qty = obj.Qty;
                objFromDb.Category = obj.Category;  


            }

        }
    }
}
