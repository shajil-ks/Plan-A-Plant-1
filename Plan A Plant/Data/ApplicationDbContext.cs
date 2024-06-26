using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Plan_A_Plant.Models;

namespace Plan_A_Plant.Data
{
    public class ApplicationDbContext:IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext>options):base(options) 
        {
            
        }

        public DbSet<Category>Categories { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ProductImage>ProductImages { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }   
        
        public DbSet<WishList> WishLists { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<Offer> Offers { get; set; }

        public DbSet<MultipleAddress> MultipleAddresses { get; set; }   

        public DbSet<WalletTransaction> WalletTransactions { get; set; }        

        //to seed data to caterory
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {       
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Indoor plants", DisplayOrder = 1, Description = "sample description",IsActive=true },
                new Category { Id = 2, Name = "Flowering plants", DisplayOrder = 2, Description = "sample description", IsActive = true },
                new Category { Id = 3, Name = "Indoor plants", DisplayOrder = 3, Description = "sample description", IsActive = true }

                );

            modelBuilder.Entity<Product>().HasData(
               new Product { Id=1, Name="Money Plant", Description=" Test msg",Price=300.0,Qty=9,CategoryId=1},
               new Product { Id=2, Name = "Money Plant", Description = " Test msg",Price=200.0,Qty=2,CategoryId=2}

               );

        }




        



    }
}
