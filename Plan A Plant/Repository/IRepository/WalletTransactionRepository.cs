using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Data;
using Plan_A_Plant.Models;

using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class WalletTransactionRepository : Repository<WalletTransaction>, IWalletTransactionRepository
    {
        private ApplicationDbContext _db;
        public WalletTransactionRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }

       

        public void Update(WalletTransaction obj)
        {
            _db.WalletTransactions.Update(obj);
      
        }


        
    }
}
