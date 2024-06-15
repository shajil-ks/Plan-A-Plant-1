using Plan_A_Plant.Data;
using Plan_A_Plant.Models;
using System.Linq.Expressions;

namespace Plan_A_Plant.Repository.IRepository
{
    public class ApplicationUserRepository : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private ApplicationDbContext _db;
        public ApplicationUserRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
            
        }


        public void Update(ApplicationUser obj)
        {
            var applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == obj.Id);
            if (applicationUser != null)
            {
                applicationUser.PhoneNumber = obj.PhoneNumber;
                applicationUser.Name = obj.Name;
                applicationUser.StreetAddress = obj.StreetAddress;
                applicationUser.City = obj.City;
                applicationUser.State = obj.State;
                applicationUser.SecurityStamp = obj.PostalCode;
                applicationUser.WalletPaymentIntentId = obj.WalletPaymentIntentId; 
                applicationUser.WalletSessionId = obj.WalletSessionId;
                applicationUser.UsedCoupons = obj.UsedCoupons;  
                if (obj.Wallet != null)
                {
                    applicationUser.Wallet = obj.Wallet;
                }
            }


        }



    }
}
