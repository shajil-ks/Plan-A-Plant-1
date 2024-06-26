using Plan_A_Plant.Data;
using Plan_A_Plant.Repository.IRepository;

namespace Plan_A_Plant.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository Category { get; private set; }
        public IProductRepository Product { get; private set; }
        public IProductImageRepository ProductImage { get; private set; }

        public IShoppingCartRepository ShoppingCart { get; private set; }   

        public IApplicationUserRepository ApplicationUser { get; private set; }

        public IOrderHeaderRepository OrderHeader { get; private set; } 

        public IOrderDetailRepository OrderDetail { get; private set; } 

        public IWishListRepository WishList { get; private set; }   

        public ICouponRepository Coupon { get; private set; }   

        public IOfferRepository Offer { get; private set; } 

        public IMultipleAddressRepository MultipleAddress { get; private set; } 

        public IWalletTransactionRepository WalletTransaction { get; private set; } 

        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            ApplicationUser = new ApplicationUserRepository(_db);
            ShoppingCart=new ShoppingCartRepository(_db);  
            ProductImage=new ProductImageRepository(_db);
            Category = new CategoryRepository(_db);
            Product = new ProductRepository(_db);
            OrderDetail = new OrderDetailRepository(_db);
            OrderHeader = new OrderHeaderRepository(_db);
            WishList = new WishListRepository(_db); 
            Coupon = new CouponRepository(_db);
            Offer = new OfferRepository(_db);
            MultipleAddress = new MultipleAddressRepository(_db); 
            WalletTransaction = new WalletTransactionRepository(_db);   
            
        }
        

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
